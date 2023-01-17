using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spice.Models
{
    public class OrderDetails
    {
        [key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual OrderHeader OrderHeader { get; set; }

        [Required]
        public int MenuItemsId { get; set; }

        [ForeignKey("MenuItemsId")]
        public virtual MenuItems MenuItems { get; set; }

        public int Count { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        [Required]
        public double Price { get; set; }
    }
}
