using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.Contracts.Common
{
    public class EventBase
    {
        public Guid f_uid { get; set; }
        public DateTime? f_create_date { get; set; }
    }
}