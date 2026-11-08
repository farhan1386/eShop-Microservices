using eShop.Shared.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.CustomerService.Models
{
    public class CustomerAddressModel : BaseModel
    {
        public Guid f_customer_uid { get; set; }
        public string f_street { get; set; }
        public string f_city { get; set; }
        public string f_state { get; set; }
        public string f_postal_code { get; set; }
    }
}
