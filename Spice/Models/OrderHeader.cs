
using Microsoft.Build.Framework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;



namespace Spice.Models
{
    public class OrderHeader
    {
        [key]
        public int Id { get; set; }

        
        [Microsoft.Build.Framework.Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        
        [Microsoft.Build.Framework.Required]
        public DateTime OrderDate { get; set; }
        [Microsoft.Build.Framework.Required]
        
        public double OrderTotalOriginal { get; set; }
        [Microsoft.Build.Framework.Required]
        [DisplayFormat(DataFormatString = "{0:C}")]
        [Display(Name ="Order Total")]
        public double OrderTotal { get; set;}


        [Microsoft.Build.Framework.Required]
       
        [Display(Name = "PickUp Time")]
        public DateTime PickUpTime { get; set; }


        [Microsoft.Build.Framework.Required]
        [NotMapped]
        public DateTime PickUpDate { get; set; }

        [Display(Name ="Coupons Code")]
        public string? CouponsCode { get; set; }
        public double? CouponsDiscount { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string ? Comments { get; set; }

        [Display(Name =" Pickup Name")]
        public String PickUpName { get; set; }

        [Display(Name = " Phone Number")]
        public String PhoneNumber { get; set; }

        
        public String? TransactionId { get; set; }
    }
}
