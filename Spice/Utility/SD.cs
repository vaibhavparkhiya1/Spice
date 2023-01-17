using Spice.Models;

namespace Spice.Utility
{
    public static class SD
    {
        public const string DefaultFoodImage = "default_food.png";
        public const string ManagerUser = "Manager";
        public const string KitchenUser = "Kitchen";
        public const string FrontDeskUser = "FrontDesk";
        public const string CustomerEndUser = "Customer";
        public const string ssShoppingCartCount = "ssCartCount";
        public const string ssCouponCode = "ssCouponCode";

        public const string StatusSubmitted = "Submitted";
        public const string StatusInProcess = "Being Prepared";
        public const string StatusReady = " Ready For PickUp";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";


        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusRejected = "Rejected";
       

        public static string ConvertToRawHtml(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }
        public static double DiscountPrice(Coupons CouponsFromDb, double OriginalOrderTotal)
        {
            if (CouponsFromDb == null)
            {
                return OriginalOrderTotal;
            }
            else
            {
                if (CouponsFromDb.MinimumAmount > OriginalOrderTotal)
                {
                    return OriginalOrderTotal;
                }
                else
                {
                    //everything is valid
                    if (Convert.ToInt32(CouponsFromDb.CouponsType) == (int)Coupons.ECouponsType.Dollar)
                    {
                        //$10 off $100
                        return Math.Round(OriginalOrderTotal - CouponsFromDb.Discount, 2);
                    }
                    if (Convert.ToInt32(CouponsFromDb.CouponsType) == (int)Coupons.ECouponsType.Percent)
                    {
                        //10% off $100
                        return Math.Round(OriginalOrderTotal - (OriginalOrderTotal * CouponsFromDb.Discount / 100), 2);
                    }
                }
            }
            return OriginalOrderTotal;
        }

    }
}
