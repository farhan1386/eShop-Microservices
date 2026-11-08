using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.Contracts.Enums
{
    public enum OrderStatus
    {
        Pending = 1,
        Processing = 2,
        Paid = 3,
        Shipped = 4,
        Cancelled = 5
    }
}
