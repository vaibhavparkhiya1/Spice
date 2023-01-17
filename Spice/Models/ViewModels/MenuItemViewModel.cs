using Microsoft.EntityFrameworkCore;

namespace Spice.Models.ViewModels
{
    public class MenuItemViewModel
    {
       

        public MenuItems? MenuItems { get; set; }

        public IEnumerable<Category>? category { get; set; }
        //public DbSet<Category>? Category { get; internal set; }
        public IEnumerable<SubCategory>? Subcategory { get; set; }

    }
}
