using System;
using System.Collections.Generic;
using System.Text;
using eShop.Shared.Common;

namespace eShop.ProductService.Models
{
    public class ProductModel : BaseModel
    {
        public Guid f_category_uid { get; set; }
        public string f_name { get; set; }
        public string f_sku { get; set; }
        public decimal f_price { get; set; }
        public string f_attributes { get; set; }
    }
}
