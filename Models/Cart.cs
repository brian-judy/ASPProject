using Newtonsoft.Json;


namespace ASPProject.Models
{
    public class Cart
    {
        [JsonProperty]
        private List<CartItem> cartItems = new();

        const int MaxQuantity = 15;

        public CartItem? GetCartItem(int? productID)
        {
            CartItem? aItem = cartItems.Where(p => p.Product?.ProductID == productID).FirstOrDefault();

            return aItem;
        }

        public void AddItem(Product aProduct)
        {
            CartItem? aItem = GetCartItem(aProduct.ProductID);

            // If it is a new item

            if (aItem == null)
            {
                cartItems.Add(new CartItem { Product = aProduct, Quantity = 1 });
            }

            else
            {
                // Increase quantity by 1 if the current quantity is less than 20

                if (aItem.Quantity < MaxQuantity)
                {
                    aItem.Quantity += 1;
                }
            }
        }

        public void UpdateItem(int? productPK, int quantity)
        {
            CartItem? aItem = GetCartItem(productPK);

            if (aItem != null)
            {
                aItem.Quantity = (quantity <= MaxQuantity) ? quantity : MaxQuantity;
            }
        }

        public void RemoveItem(int? productID)
        {
            cartItems.RemoveAll(r => r.Product?.ProductID == productID);
        }

        public void ClearCart()
        {
            cartItems.Clear();
        }

        public decimal? ComputeOrderTotal()
        {
            return cartItems.Sum(s => s.Product?.UnitPrice * s.Quantity);
        }

        public IEnumerable<CartItem> CartItems()
        {
            return cartItems;
        }
    }
}
