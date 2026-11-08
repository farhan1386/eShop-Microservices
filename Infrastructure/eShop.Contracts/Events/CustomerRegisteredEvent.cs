using eShop.Contracts.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.Contracts.Events
{
    public class CustomerRegisteredEvent : IntegrationEvent
    {
        public Guid CustomerId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
