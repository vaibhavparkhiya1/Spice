using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;
using Stripe;
using System.Security.Claims;

namespace Spice.Areas.Customers.Controllers
{
    [Area("Customers")]
    public class CartController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
       
        [BindProperty]
        public OrderDetailsCart detailsCart { get; set; }
        public CartController(ApplicationDbContext db,IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Index()
        {
            detailsCart = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader()
            };
            detailsCart.OrderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            var cart = _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value);
            if (cart != null)
            {
                detailsCart.ListCart = cart.ToList();
            }

            foreach (var list in detailsCart.ListCart)
            {
                list.MenuItems = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == list.MenuItemsId);
                detailsCart.OrderHeader.OrderTotal = detailsCart.OrderHeader.OrderTotal + (list.MenuItems.Price * list.Count);
                list.MenuItems.Description = SD.ConvertToRawHtml(list.MenuItems.Description);
                if (list.MenuItems.Description.Length > 100)
                {
                    list.MenuItems.Description = list.MenuItems.Description.Substring(0, 99) + "...";
                }
            }
            detailsCart.OrderHeader.OrderTotalOriginal = detailsCart.OrderHeader.OrderTotal;

            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                detailsCart.OrderHeader.CouponsCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupons.Where(c => c.Name.ToLower() == detailsCart.OrderHeader.CouponsCode.ToLower()).FirstOrDefaultAsync();
                detailsCart.OrderHeader.OrderTotal = SD.DiscountPrice(couponFromDb, detailsCart.OrderHeader.OrderTotalOriginal);
            }

            return View(detailsCart);

        }
        public async Task<IActionResult> Summary()
        {

            detailsCart = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader()
            };

            detailsCart.OrderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ApplicationUser applicationUser = await _db.ApplicationUsers.Where(c => c.Id == claim.Value).FirstOrDefaultAsync();
            var cart = _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value);
            if (cart != null)
            {
                detailsCart.ListCart = cart.ToList();
            }

            foreach (var list in detailsCart.ListCart)
            {
                list.MenuItems = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == list.MenuItemsId);
                detailsCart.OrderHeader.OrderTotal = detailsCart.OrderHeader.OrderTotal + (list.MenuItems.Price * list.Count);

            }
            detailsCart.OrderHeader.OrderTotalOriginal = detailsCart.OrderHeader.OrderTotal;
            detailsCart.OrderHeader.PickUpName = applicationUser.Name;
            detailsCart.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
            detailsCart.OrderHeader.PickUpTime = DateTime.Now;


            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                detailsCart.OrderHeader.CouponsCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupons.Where(c => c.Name.ToLower() == detailsCart.OrderHeader.CouponsCode.ToLower()).FirstOrDefaultAsync();
                detailsCart.OrderHeader.OrderTotal = SD.DiscountPrice(couponFromDb, detailsCart.OrderHeader.OrderTotalOriginal);
            }


            return View(detailsCart);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            detailsCart.ListCart = await _db.ShoppingCart.Where(u=>u.ApplicationUserId == claim.Value).ToListAsync();

            detailsCart.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            detailsCart.OrderHeader.OrderDate = DateTime.Now;
            detailsCart.OrderHeader.UserId = claim.Value;
            detailsCart.OrderHeader.Status = SD.PaymentStatusPending;
            detailsCart.OrderHeader.PickUpTime = Convert.ToDateTime(detailsCart.OrderHeader.PickUpDate.ToShortDateString()+ " " + detailsCart.OrderHeader.PickUpTime.ToShortTimeString());

            List<OrderDetails>orderDetailsList = new List<OrderDetails>();
            _db.OrderHeader.Add(detailsCart.OrderHeader);
            await _db.SaveChangesAsync();

            detailsCart.OrderHeader.OrderTotalOriginal = 0;
            ApplicationUser applicationUser = await _db.ApplicationUsers.Where(c => c.Id == claim.Value).FirstOrDefaultAsync();



            foreach (var item in detailsCart.ListCart)
            {
                item.MenuItems = await _db.MenuItems.FirstOrDefaultAsync(m => m.Id == item.MenuItemsId);

                OrderDetails orderDetails = new OrderDetails
                {
                    MenuItemsId = item.MenuItemsId,
                    OrderId = detailsCart.OrderHeader.Id,
                    Description = item.MenuItems.Description,
                    Name = item.MenuItems.Name,
                    Price = item.MenuItems.Price,
                    Count = item.Count

                };
                detailsCart.OrderHeader.OrderTotalOriginal += orderDetails.Count * orderDetails.Price;
                _db.OrderDetails.Add(orderDetails); 
                
            }


            if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
            {
                detailsCart.OrderHeader.CouponsCode = HttpContext.Session.GetString(SD.ssCouponCode);
                var couponFromDb = await _db.Coupons.Where(c => c.Name.ToLower() == detailsCart.OrderHeader.CouponsCode.ToLower()).FirstOrDefaultAsync();
                detailsCart.OrderHeader.OrderTotal = SD.DiscountPrice(couponFromDb, detailsCart.OrderHeader.OrderTotalOriginal);
            }
            else
            {
                detailsCart.OrderHeader.OrderTotal = detailsCart.OrderHeader.OrderTotalOriginal;
            }
            detailsCart.OrderHeader.CouponsDiscount = detailsCart.OrderHeader.OrderTotalOriginal - detailsCart.OrderHeader.OrderTotal ;
            
            _db.ShoppingCart.RemoveRange(detailsCart.ListCart);
            HttpContext.Session.SetInt32(SD.ssShoppingCartCount, 0);
            await _db.SaveChangesAsync();

            var options = new ChargeCreateOptions
            {
                Amount = Convert.ToInt32(detailsCart.OrderHeader.OrderTotal * 100),
                Currency = "usd",
                Description = "Order ID " + detailsCart.OrderHeader.Id,
                Source = stripeToken


            };
            var service = new ChargeService();
            Charge charge = service.Create(options);
             if(charge.BalanceTransactionId == null)
            {
                detailsCart.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }
            else
            {
                detailsCart.OrderHeader.TransactionId= charge.BalanceTransactionId;
            }
            if(charge.Status.ToLower() == "succeeded") 
            {
                await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == claim.Value)
                    .FirstOrDefault().Email, "Spice - Order Created" 
                    + detailsCart.OrderHeader.Id.ToString(), "Order Has Been Submitted Successfully");


                detailsCart.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                detailsCart.OrderHeader.Status = SD.StatusSubmitted;
            }
            else
            {
                detailsCart.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }



            await _db.SaveChangesAsync();
            //return RedirectToAction("Index","Home");
            return RedirectToAction("Confirm", "Order", new { id = detailsCart.OrderHeader.Id });

        }
        public IActionResult AddCoupon()
        {
            if (detailsCart.OrderHeader.CouponsCode == null)
            {
                detailsCart.OrderHeader.CouponsCode = "";
            }
            HttpContext.Session.SetString(SD.ssCouponCode, detailsCart.OrderHeader.CouponsCode);

            return RedirectToAction(nameof(Index));
        }
        public IActionResult RemoveCoupon()
        {

            HttpContext.Session.SetString(SD.ssCouponCode, String.Empty);

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> plus(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);
            cart.Count += 1;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> minus(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);
            if (cart.Count == 1)
            {
                _db.ShoppingCart.Remove(cart);
                await _db.SaveChangesAsync();
                var cnt = _db.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);
            }
            else
            {
                cart.Count -= 1;
                await _db.SaveChangesAsync();

            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);

            _db.ShoppingCart.Remove(cart);
            await _db.SaveChangesAsync();


            var cnt = _db.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
            HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        
    }
}
