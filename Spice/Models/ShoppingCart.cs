using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spice.Models
{
    public class ShoppingCart
    {
        public ShoppingCart()
        {
            Count = 1;
        }
      
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }
        [NotMapped]
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        public int MenuItemsId { get; set; }
        [NotMapped]
        [ForeignKey("MenuItemsId")]
        public virtual MenuItems MenuItems { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = " Please enter a value grater then or equal to {1} ")]
        public int Count { get; set; }

    }
}
