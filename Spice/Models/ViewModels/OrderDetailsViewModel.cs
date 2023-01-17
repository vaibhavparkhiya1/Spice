namespace Spice.Models.ViewModels
{
    public class OrderDetailsViewModel
    {
        public OrderHeader orderHeader { get; set; }
        public List<OrderDetails> orderDetails { get; set; }
    }
}
