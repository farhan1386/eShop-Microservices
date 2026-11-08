using eShop.Shared.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.AuthService.Models
{
    public class UserRoleModel : BaseModel
    {
        public Guid f_user_credentials_uid { get; set; }
        public Guid f_role_uid { get; set; }
    }
}