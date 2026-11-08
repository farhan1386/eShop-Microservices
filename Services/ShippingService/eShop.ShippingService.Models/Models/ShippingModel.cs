using eShop.Shared.Common;
using System;

namespace eShop.ShippingService.Models
{
    public class ShippingModel : BaseModel
    {
        public Guid f_order_uid { get; set; }
        public string f_carrier_name { get; set; }
        public string? f_tracking_number { get; set; }
        public string f_shipping_status { get; set; }
        public DateTime? f_estimated_delivery { get; set; }
    }
}