using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.Contracts.Common
{
    public abstract class IntegrationEvent : EventBase
    {
        public Guid CorrelationId { get; set; }
    }
}
