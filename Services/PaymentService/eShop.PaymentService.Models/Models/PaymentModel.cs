using eShop.Contracts.Enums;
using eShop.Shared.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.PaymentService.Models
{
    public class PaymentModel : BaseModel
    {
        public Guid f_order_uid { get; set; }
        public decimal f_amount { get; set; }
        public string f_currency { get; set; } = "INR";
        public string f_method { get; set; } = string.Empty;
        public string f_gateway_name { get; set; } = string.Empty;
        public PaymentStatus f_status { get; set; }
        public string? f_gateway_txid { get; set; }
        public DateTime? f_payment_date { get; set; }
    }
}
