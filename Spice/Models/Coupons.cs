using Microsoft.Build.Framework;

namespace Spice.Models
{
    public class Coupons
    {
        [key]
        public int Id { get; set; }

        [Required]
        public string ? Name { get; set; }
        
        [Required]
        public String? CouponsType { get; set; }

        public enum ECouponsType {Percent = 0 , Dollar = 1 }

        [Required]
        public double Discount { get; set; }

        [Required]
        public double  MinimumAmount { get; set; }


        public byte[]? Picture { get; set; }

        public bool IsActive { get; set; }
    }
}
