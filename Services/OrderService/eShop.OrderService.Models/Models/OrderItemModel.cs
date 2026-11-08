using eShop.Shared.Common;
using System;

namespace eShop.OrderService.Models
{
    public class OrderItemModel : BaseModel
    {
        public Guid f_order_uid { get; set; }
        public Guid f_product_uid { get; set; }
        public int f_quantity { get; set; }
        public decimal f_unit_price { get; set; }
    }
}