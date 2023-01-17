using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models.ViewModels;
using Spice.Utility;
using System.Data;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using Spice.Models;


namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]

    public class MenuItemsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hostingEnvironment;

        [BindProperty]
        public MenuItemViewModel MenuItemsVM { get; set; }

        public MenuItemsController(ApplicationDbContext db, IWebHostEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            MenuItemsVM = new MenuItemViewModel()
            {
                category = _db.Category,
                MenuItems = new Models.MenuItems()
            };
        }

        public async Task<IActionResult> Index()
        {
            var menuItems = await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync();
            return View(menuItems);
        }

        //GET - CREATE
        public IActionResult Create()
        {
            return View(MenuItemsVM);
        }

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            MenuItemsVM.MenuItems.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            //if (!ModelState.IsValid)
            //{
            //    return View(MenuItemVM);
            //}

            _db.MenuItems.Add(MenuItemsVM.MenuItems);
            await _db.SaveChangesAsync();

            //Work on the image saving section

            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemsFromDb = await _db.MenuItems.FindAsync(MenuItemsVM.MenuItems.Id);
           
            if (files.Count > 0)
            {
                //files has been uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);

                using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemsVM.MenuItems.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }
                menuItemsFromDb.Image = @"\images\" + MenuItemsVM.MenuItems.Id + extension;
            }
            else
            {
                //no file was uploaded, so use default
                var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemsVM.MenuItems.Id + ".png");
                menuItemsFromDb.Image = @"\images\" + MenuItemsVM.MenuItems.Id + ".png";
            }

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        //GET - EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemsVM.MenuItems = await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            MenuItemsVM.Subcategory= await _db.SubCategory.Where(s => s.CategoryId == MenuItemsVM.MenuItems.CategoryId).ToListAsync();

            if (MenuItemsVM.MenuItems == null)
            {
                return NotFound();
            }
            return View(MenuItemsVM);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPOST(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemsVM.MenuItems.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            //if (!ModelState.IsValid)
            //{
            //    MenuItemVM.Subcategory = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItems.CategoryId).ToListAsync();
            //    return View(MenuItemVM);
            //}

            //Work on the image saving section

            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFromDb = await _db.MenuItems.FindAsync(MenuItemsVM.MenuItems.Id);

            if (files.Count > 0)
            {
                //New Image has been uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var extension_new = Path.GetExtension(files[0].FileName);

                //Delete the original file
                var imagePath = Path.Combine(webRootPath, menuItemFromDb.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                //we will upload the new file
                using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemsVM.MenuItems.Id + extension_new), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }
                menuItemFromDb.Image = @"\images\" + MenuItemsVM.MenuItems.Id + extension_new;
            }

            menuItemFromDb.Name = MenuItemsVM.MenuItems.Name;
            menuItemFromDb.Description = MenuItemsVM.MenuItems.Description;
            menuItemFromDb.Price = MenuItemsVM.MenuItems.Price;
            menuItemFromDb.Spicyness = MenuItemsVM.MenuItems.Spicyness;
            menuItemFromDb.CategoryId = MenuItemsVM.MenuItems.CategoryId;
            menuItemFromDb.SubCategoryId = MenuItemsVM.MenuItems.SubCategoryId;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //GET : Details MenuItem
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemsVM.MenuItems = await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);

            if (MenuItemsVM.MenuItems == null)
            {
                return NotFound();
            }

            return View(MenuItemsVM);
        }

        //GET : Delete MenuItem
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemsVM.MenuItems = await _db.MenuItems.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);

            if (MenuItemsVM.MenuItems == null)
            {
                return NotFound();
            }

            return View(MenuItemsVM);
        }

        //POST Delete MenuItem
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            MenuItems menuItem = await _db.MenuItems.FindAsync(id);

            if (menuItem != null)
            {
                var imagePath = Path.Combine(webRootPath, menuItem.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                _db.MenuItems.Remove(menuItem);
                await _db.SaveChangesAsync();

            }

            return RedirectToAction(nameof(Index));
        }

    }
}
