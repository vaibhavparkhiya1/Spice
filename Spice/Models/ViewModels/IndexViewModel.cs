

namespace Spice.Models.ViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<MenuItems> MenuItems { get; set; }
        public IEnumerable<Category> Category { get; set; }

        public IEnumerable<Coupons> Coupons { get; set; }
    }
}
