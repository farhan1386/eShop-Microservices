using eShop.Shared.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.AuthService.Models
{
    public class RefreshTokenModel : BaseModel
    {
        public Guid f_user_credentials_uid { get; set; }
        public string f_token { get; set; }
        public DateTime f_expires_date { get; set; }
        public bool f_is_revoked { get; set; }
    }
}