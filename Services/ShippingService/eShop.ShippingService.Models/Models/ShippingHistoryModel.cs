using eShop.Shared.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.ShippingService.Models
{
    public class ShippingHistoryModel : BaseModel
    {
        public Guid f_shipping_manifest_uid { get; set; }
        public string f_old_status { get; set; }
        public string f_new_status { get; set; }
        public string f_status_location { get; set; }
        public string f_comment { get; set; }
    }
}