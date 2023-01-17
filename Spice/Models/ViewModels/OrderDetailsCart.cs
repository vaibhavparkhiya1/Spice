namespace Spice.Models.ViewModels
{
    public class OrderDetailsCart
    {
        internal List<ShoppingCart> listCart;

        public List<ShoppingCart>ListCart { get; set; }
		public OrderDetailsCart detailsCart { get; set; }
		public OrderHeader OrderHeader { get; set; }    
    }
}
