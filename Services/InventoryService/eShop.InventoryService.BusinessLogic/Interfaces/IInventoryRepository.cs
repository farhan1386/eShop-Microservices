using eShop.InventoryService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.InventoryService.BusinessLogic.Interfaces
{
    public interface IInventoryRepository
    {
        Task<IEnumerable<InventoryStockModel>> Get();
        Task<InventoryStockModel?> Find(Guid uid);
        Task<InventoryStockModel?> FindByProduct(Guid productUid);
        Task<InventoryStockModel> Add(InventoryStockModel model);
        Task<InventoryStockModel?> Update(InventoryStockModel model);
        Task<int> Remove(InventoryStockModel model);
    }
}
