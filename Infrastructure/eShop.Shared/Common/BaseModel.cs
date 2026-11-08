using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.Shared.Common
{
    public class BaseModel
    {
        public Guid f_uid { get; set; }
        public int f_iid { get; set; }
        public DateTime? f_create_date { get; set; }
        public Guid? f_create_by { get; set; }
        public DateTime? f_update_date { get; set; }
        public Guid? f_update_by { get; set; }
        public DateTime? f_delete_date { get; set; }
        public Guid? f_delete_by { get; set; }
        public int RowCount { get; set; }
    }
}
