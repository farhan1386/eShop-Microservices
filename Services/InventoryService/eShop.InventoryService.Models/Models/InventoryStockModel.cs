using eShop.Shared.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.InventoryService.Models
{
    public class InventoryStockModel : BaseModel
    {
        public Guid f_product_uid { get; set; }
        public int f_quantity { get; set; }
        public int f_reserved { get; set; }
        public byte[]? f_row_version { get; set; }

        public int AvailableQuantity => f_quantity - f_reserved;
    }
}
