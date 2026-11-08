using eShop.Shared.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.CartService.Models
{
    public class CartModel : BaseModel
    {
        public Guid f_customer_uid { get; set; }
    }
}
