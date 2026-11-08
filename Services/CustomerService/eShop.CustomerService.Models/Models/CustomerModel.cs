using eShop.Shared.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.CustomerService.Models
{
    public class CustomerModel : BaseModel
    {
        public string f_email { get; set; }
        public string f_password { get; set; }
        public string f_fname { get; set; }
        public string f_lname { get; set; }
        public string f_phone { get; set; }
    }
}
