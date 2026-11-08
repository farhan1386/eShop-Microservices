using eShop.Shared.Common;
using System;

namespace eShop.OrderService.Models
{
    public class OrderHistoryModel : BaseModel
    {
        public Guid f_order_uid { get; set; }
        public string f_old_status { get; set; }
        public string f_new_status { get; set; }
        public string? f_comment { get; set; }
    }
}