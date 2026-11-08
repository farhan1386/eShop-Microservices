using eShop.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.Contracts.Messages
{
    public class SmsMessage : EventBase
    {
        public string PhoneNumber { get; set; }
        public string MessageText { get; set; }
    }
}
