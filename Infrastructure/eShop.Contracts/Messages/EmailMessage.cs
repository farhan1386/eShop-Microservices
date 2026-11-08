using eShop.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.Contracts.Messages
{
    public class EmailMessage : EventBase
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
