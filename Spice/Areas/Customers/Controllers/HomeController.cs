using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;
using System.Diagnostics;
using System.Security.Claims;

namespace Spice.Areas.Customers.Controllers
{
    [Area("Customers")]

    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            IndexViewModel IndexVM = new IndexViewModel()
            {
               MenuItems  = await _db.MenuItems.Include(m=>m.Category).Include(m=>m.SubCategory).ToListAsync(),
               Category = await _db.Category.ToListAsync(),
               Coupons = await _db.Coupons.Where(c=>c.IsActive==true).ToListAsync(),

            };
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                var cnt = _db.ShoppingCart.Where(u => u.ApplicationUserId == claim.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, cnt);
            }


            return View(IndexVM);
                 
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var menuitemsFromDb = await _db.MenuItems.Include(m=>m.Category).Include(m=>m.SubCategory).Where(m=>m.Id==id).FirstOrDefaultAsync();

            ShoppingCart CartObject = new ShoppingCart()
            {
                MenuItems = menuitemsFromDb,
                MenuItemsId = menuitemsFromDb.Id
            };
            return View(CartObject);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart CartObject)
         {
            CartObject.Id = 0;

            if(!ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                CartObject.ApplicationUserId = claim.Value;


                //ShoppingCart CartFromdb = await _db.ShoppingCart.Where(c => c.ApplicationUserId == Cartobject.ApplicationUserId
                //                                                       && c.MenuItemsId == Cartobject.MenuItemsId).FirstOrDefaultAsync(); 

                ShoppingCart CartFromdb = await _db.ShoppingCart.Where(c => c.ApplicationUserId == CartObject.ApplicationUserId 
				&&c.MenuItemsId == CartObject.MenuItemsId).FirstOrDefaultAsync();

                if(CartFromdb == null)
                {
                    await _db.ShoppingCart.AddAsync(CartObject);
                }
                else
                {
                        CartFromdb.Count = CartFromdb.Count + CartObject.Count; 
                }
                await _db.SaveChangesAsync();
                var count = _db.ShoppingCart.Where(c => c.ApplicationUserId == CartObject.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32(SD.ssShoppingCartCount, count);

                return RedirectToAction("Index");
            }
            else
            {
                var menuitemsFromDb = await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.Id == CartObject.MenuItemsId).FirstOrDefaultAsync();

                ShoppingCart cartobj = new ShoppingCart()
                {
                    MenuItems = menuitemsFromDb,
                    MenuItemsId = menuitemsFromDb.Id
                };
                return View(cartobj);
            }


        }



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}