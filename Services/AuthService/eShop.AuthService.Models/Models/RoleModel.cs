using eShop.Shared.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.AuthService.Models
{
    public class RoleModel : BaseModel
    {
        public string f_name { get; set; }
        public string? f_description { get; set; }
    }
}
