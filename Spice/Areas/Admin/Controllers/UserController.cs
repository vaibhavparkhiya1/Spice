using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Utility;
using System.Security.Claims;

namespace Spice.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerUser)]
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UserController(ApplicationDbContext db)
        {
            _db= db;
        }

         public async  Task<IActionResult> Index()
         {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);


            return View(await _db.ApplicationUsers.Where(u => u.Id != claim.Value).ToListAsync());
         }
        public async Task<IActionResult> Lock(string id)
        {
             if(id == null)
             {
                return NotFound();
             }
             var ApplicationUser = await _db.ApplicationUsers.FirstOrDefaultAsync(u=>u.Id== id);

            if(ApplicationUser == null) 
            { 
            
            return NotFound();
            }
            ApplicationUser.LockoutEnd = DateTime.Now.AddYears(1000);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Unlock(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var ApplicationUser = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == id);

            if (ApplicationUser == null)
            {

                return NotFound();
            }
            ApplicationUser.LockoutEnd = DateTime.Now;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
