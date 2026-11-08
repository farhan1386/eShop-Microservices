using eShop.Shared.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.CartService.Models
{
    public class CartItemModel : BaseModel
    {
        public Guid f_cart_uid { get; set; }
        public Guid f_product_uid { get; set; }
        public int f_quantity { get; set; }
        public decimal f_price_snapshot { get; set; }
    }
}
