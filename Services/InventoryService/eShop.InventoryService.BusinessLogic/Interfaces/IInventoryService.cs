using eShop.InventoryService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.InventoryService.BusinessLogic.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryStockModel>> GetAllAsync();
        Task<InventoryStockModel?> GetByUidAsync(Guid uid);
        Task<InventoryStockModel?> GetByProductAsync(Guid productUid);
        Task<InventoryStockModel> InitializeStockAsync(InventoryStockModel stock);
        Task<InventoryStockModel?> RestockAsync(Guid productUid, int quantityToAdd);
        Task<InventoryStockModel?> ReserveStockAsync(Guid productUid, int quantity);
        Task<InventoryStockModel?> ReleaseReservedStockAsync(Guid productUid, int quantity);
        Task<InventoryStockModel?> ConfirmDeductStockAsync(Guid productUid, int quantity);
        Task<int> DeleteAsync(Guid uid);
    }
}
