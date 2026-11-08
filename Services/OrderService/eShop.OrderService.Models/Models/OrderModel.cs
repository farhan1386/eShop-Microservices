using eShop.Contracts.Enums;
using eShop.Shared.Common;
using System;

namespace eShop.OrderService.Models
{
    public class OrderModel : BaseModel
    {
        public Guid f_customer_uid { get; set; }
        public DateTime f_order_date { get; set; }
        public decimal f_total_price { get; set; }
        public OrderStatus f_status { get; set; }
    }
}