using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Utility;
using System.Data;

namespace Spice.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = SD.ManagerUser)]
    public class CouponsController : Controller
    {
       

        private readonly ApplicationDbContext _db;

        public CouponsController(ApplicationDbContext db)
        {
            _db= db;
        }


        public async Task<IActionResult> Index()
        {
            return View(await _db.Coupons.ToListAsync());
        }

        public IActionResult Create() 
        {
          
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupons coupons)
        {   
            var files = HttpContext.Request.Form.Files;
            if (files.Count > 0)
            {
               byte[]? p1 = null;
               using (var fs1 = files[0].OpenReadStream())
               {
                   using (var ms1 = new MemoryStream())
                   {
                     fs1.CopyTo(ms1);
                     p1 = ms1.ToArray();
                   }
               }
                    coupons.Picture = p1;
            }
                _db.Coupons.Add(coupons);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));


        }

        public async Task <IActionResult> Edit (int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
             var coupons = await _db.Coupons.SingleOrDefaultAsync(m=>m.Id==id); 
            
            if(coupons == null)
            {
                return NotFound();
            }
            return View(coupons);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Coupons coupons)
        {
            if(coupons.Id == 0)
            {
                return NotFound();
            }

            var couponsFromDb = await _db.Coupons.Where(c=> c.Id==coupons.Id).FirstOrDefaultAsync();
            
            var files = HttpContext.Request.Form.Files;
            if (files.Count > 0)
            {
                byte[]? p1 = null;
                using (var fs1 = files[0].OpenReadStream())
                {
                    using (var ms1 = new MemoryStream())
                    {
                        fs1.CopyTo(ms1);
                        p1 = ms1.ToArray();
                    }
                }
                coupons.Picture = p1;
            }
               couponsFromDb.MinimumAmount = coupons.MinimumAmount;
               couponsFromDb.Name = coupons.Name;
               couponsFromDb.Discount = coupons.Discount;
               couponsFromDb.CouponsType = coupons.CouponsType;
               couponsFromDb.IsActive = coupons.IsActive;


            _db.Coupons.Add(coupons);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));


        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coupon = await _db.Coupons.FirstOrDefaultAsync(m => m.Id == id);
            if (coupon == null)
            {
                return NotFound();
            }

            return View(coupon);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var coupon = await _db.Coupons.SingleOrDefaultAsync(m => m.Id == id);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var coupons = await _db.Coupons.SingleOrDefaultAsync(m => m.Id == id);
            _db.Coupons.Remove(coupons);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

    }
}
