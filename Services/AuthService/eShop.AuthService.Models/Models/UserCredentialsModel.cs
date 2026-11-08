using eShop.Shared.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.AuthService.Models
{
    public class UserCredentialsModel : BaseModel
    {
        public Guid f_customer_uid { get; set; }
        public string f_username { get; set; }
        public string f_password_hash { get; set; }
        public string f_salt { get; set; }
        public bool f_is_locked_out { get; set; }
        public int f_failed_login_attempts { get; set; }
        public DateTime? f_last_login_date { get; set; }
    }
}
