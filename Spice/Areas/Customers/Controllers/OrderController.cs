using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;
using System.Security.Claims;
using System.Text;

namespace Spice.Areas.Customers.Controllers
{
    [Area("Customers")]
    public class OrderController : Controller
    {
        private readonly IEmailSender _emailSender;
        private ApplicationDbContext _db;

        private int PageSize = 4;
        public OrderController(ApplicationDbContext db,IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }

        [Authorize]

        public async Task<IActionResult> Confirm(int id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
            {
                orderHeader = await _db.OrderHeader.Include(o => o.ApplicationUser).FirstOrDefaultAsync(o => o.Id == id && o.UserId == claim.Value),
                orderDetails = await _db.OrderDetails.Where(o => o.OrderId == id).ToListAsync(),

            };
            return View(orderDetailsViewModel);

        }

        [Authorize]
        public async Task<IActionResult> OrderHistory(int productPage = 1)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            OrderListViewModel orderListVM = new OrderListViewModel()
            {
                Orders = new List<OrderDetailsViewModel>()
            };



            List<OrderHeader> OrderHeaderList = await _db.OrderHeader.Include(o => o.ApplicationUser).Where(u => u.UserId == claim.Value).ToListAsync();

            foreach (OrderHeader item in OrderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    orderHeader = item,
                    orderDetails = await _db.OrderDetails.Where(o => o.OrderId == item.Id).ToListAsync()
                };
                orderListVM.Orders.Add(individual);
            }

            var count = orderListVM.Orders.Count;
            orderListVM.Orders = orderListVM.Orders.OrderByDescending(p => p.orderHeader.Id)
                                 .Skip((productPage - 1) * PageSize)
                                 .Take(PageSize).ToList();

            orderListVM.pagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItem = count,
                urlParam = "/Customers/Order/OrderHistory?productPage=:"
            };

            return View(orderListVM);
        }




        public async Task<IActionResult> GetOrderDetails(int Id)
        {
            OrderDetailsViewModel orderDetailsViewModel = new OrderDetailsViewModel()
            {
                orderHeader = await _db.OrderHeader.FirstOrDefaultAsync(m => m.Id == Id),
                orderDetails = await _db.OrderDetails.Where(m => m.OrderId == Id).ToListAsync()


            };
            orderDetailsViewModel.orderHeader.ApplicationUser = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == orderDetailsViewModel.orderHeader.UserId);

            return PartialView("_IndividualOrderDetails", orderDetailsViewModel);


        }
        public IActionResult GetOrderStatus(int Id)
        {
            return PartialView("_OrderStatus", _db.OrderHeader.Where(m => m.Id == Id).FirstOrDefault().Status);

        }
        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> ManageOrder()
        {

            List<OrderDetailsViewModel> orderDetailsVM = new List<OrderDetailsViewModel>();

            List<OrderHeader> OrderHeaderList = await _db.OrderHeader.Where(o => o.Status == SD.StatusSubmitted || o.Status == SD.StatusInProcess).OrderByDescending(u => u.PickUpTime).ToListAsync();


            foreach (OrderHeader item in OrderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    orderHeader = item,
                    orderDetails = await _db.OrderDetails.Where(o => o.OrderId == item.Id).ToListAsync()
                };
                orderDetailsVM.Add(individual);
            }



            return View(orderDetailsVM.OrderBy(o => o.orderHeader.PickUpTime).ToList());
        }

        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> OrderPrepare(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusInProcess;
            await _db.SaveChangesAsync();
            return RedirectToAction("ManageOrder", "Order");
        }


        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> OrderReady(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusReady;
            await _db.SaveChangesAsync();

			//Email logic to notify user that order is ready for pickup
			await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == orderHeader.UserId)
				   .FirstOrDefault().Email, "Spice - Order Ready For PickUp"
				   + orderHeader.Id.ToString(), "Order is Ready For PickUp");

			return RedirectToAction("ManageOrder", "Order");
        }


        [Authorize(Roles = SD.KitchenUser + "," + SD.ManagerUser)]
        public async Task<IActionResult> OrderCancel(int OrderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(OrderId);
            orderHeader.Status = SD.StatusCancelled;
            await _db.SaveChangesAsync();
			await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == orderHeader.UserId)
				   .FirstOrDefault().Email, "Spice - Order Cancelled"
				   + orderHeader.Id.ToString(), "Order Has Been Cancelled Successfully");

			return RedirectToAction("ManageOrder", "Order");
        }
        [Authorize]
        public async Task<IActionResult> OrderPickUp(int productPage = 1, string searchName = null, string searchPhone = null, string searchEmail = null)
        {
            //var claimsIdentity = (ClaimsIdentity)User.Identity;
            //var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            OrderListViewModel orderListVM = new OrderListViewModel()
            {
                Orders = new List<OrderDetailsViewModel>()
            };

            StringBuilder param = new StringBuilder();
            param.Append("/Customers/Order/OrderPickUp?productPage=:");
            param.Append("&searchName=");
            if (searchName != null)
            {
                param.Append(searchName);
            }
            param.Append("&searchPhone=");
            if (searchPhone != null)
            {
                param.Append(searchPhone);
            }
            param.Append("&searchEmail=");
            if (searchEmail != null)
            {
                param.Append(searchEmail);
            }


            List<OrderHeader> OrderHeaderList = new List<OrderHeader>();
            if (searchPhone != null || searchPhone != null || searchEmail != null)
            {
                var user = new ApplicationUser();



                if (searchName != null)
                {
                    OrderHeaderList = await _db.OrderHeader.Include(o => o.ApplicationUser)
                        .Where(u => u.PickUpName.ToLower().Contains(searchName.ToLower()))
                        .OrderByDescending(o => o.OrderDate).ToListAsync();
                }
                else
                {
                    if (searchEmail != null)
                    {

                        user = await _db.ApplicationUsers.Where(u => u.Email.ToLower().Contains(searchEmail.ToLower())).FirstOrDefaultAsync();
                        OrderHeaderList = await _db.OrderHeader.Include(o => o.ApplicationUser).Where(u => u.UserId == user.Id).OrderByDescending(o => o.OrderDate).ToListAsync();


                    }
                    else
                    {
                        if (searchPhone != null)
                        {
                            OrderHeaderList = await _db.OrderHeader.Include(o => o.ApplicationUser)
                                .Where(u => u.PhoneNumber.Contains(searchPhone))
                                .OrderByDescending(o => o.OrderDate).ToListAsync();
                        }
                    }
                }
            }
            else
            {
                OrderHeaderList = await _db.OrderHeader.Include(o => o.ApplicationUser)
                   .Where(u => u.Status == SD.StatusReady).ToListAsync();
            }
            foreach (OrderHeader item in OrderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    orderHeader = item,
                    orderDetails = await _db.OrderDetails.Where(o => o.OrderId == item.Id).ToListAsync()
                };
                orderListVM.Orders.Add(individual);
            }

            var count = orderListVM.Orders.Count;
            orderListVM.Orders = orderListVM.Orders.OrderByDescending(p => p.orderHeader.Id)
                                     .Skip((productPage - 1) * PageSize)
                                     .Take(PageSize).ToList();

            orderListVM.pagingInfo = new PagingInfo
            {
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItem = count,
                urlParam = param.ToString()
            };

            return View(orderListVM);
        }
        [Authorize(Roles =SD.ManagerUser + "," +SD.FrontDeskUser)]
        [HttpPost]
        [ActionName("OrderPickup")]
        public async Task<IActionResult> OrderPickuppost(int orderId)
        {
            OrderHeader orderHeader = await _db.OrderHeader.FindAsync(orderId);
            orderHeader.Status = SD.StatusCompleted;
            await _db.SaveChangesAsync();
			await _emailSender.SendEmailAsync(_db.Users.Where(u => u.Id == orderHeader.UserId)
				   .FirstOrDefault().Email, "Spice - Order Completed"
				   + orderHeader.Id.ToString(), "Order Has Been Completed Successfully");


			return RedirectToAction("OrderPickup", "Order");


        }



        public IActionResult Index()
        {
            return View();
        }
    }
}
