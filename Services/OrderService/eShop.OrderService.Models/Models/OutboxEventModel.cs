using eShop.Shared.Common;
using System;

namespace eShop.OrderService.Models
{
    public class OutboxEventModel : BaseModel
    {
        public string f_event_type { get; set; }
        public string f_payload { get; set; }
        public string f_status { get; set; } = "Pending";
        public DateTime? f_processed_date { get; set; }
        public string? f_error_log { get; set; }
    }
}