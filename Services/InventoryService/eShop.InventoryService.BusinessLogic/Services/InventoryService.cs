using eShop.InventoryService.BusinessLogic.Interfaces;
using eShop.InventoryService.Models;

namespace eShop.InventoryService.BusinessLogic.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public Task<IEnumerable<InventoryStockModel>> GetAllAsync()
        {
            return _inventoryRepository.Get();
        }

        public Task<InventoryStockModel?> GetByUidAsync(Guid uid)
        {
            return _inventoryRepository.Find(uid);
        }

        public Task<InventoryStockModel?> GetByProductAsync(Guid productUid)
        {
            return _inventoryRepository.FindByProduct(productUid);
        }

        public async Task<InventoryStockModel> InitializeStockAsync(InventoryStockModel stock)
        {
            var existing = await _inventoryRepository.FindByProduct(stock.f_product_uid);
            if (existing is not null)
            {
                throw new InvalidOperationException($"Inventory record already exists for product '{stock.f_product_uid}'.");
            }

            if (stock.f_quantity < 0)
            {
                throw new InvalidOperationException("Initial stock quantity cannot be negative.");
            }

            stock.f_reserved = 0;
            return await _inventoryRepository.Add(stock);
        }

        public async Task<InventoryStockModel?> RestockAsync(Guid productUid, int quantityToAdd)
        {
            if (quantityToAdd <= 0)
            {
                throw new InvalidOperationException("Restock quantity must be greater than zero.");
            }

            var existing = await _inventoryRepository.FindByProduct(productUid);
            if (existing is null)
            {
                return null;
            }

            existing.f_quantity += quantityToAdd;

            var updated = await _inventoryRepository.Update(existing);
            if (updated is null)
            {
                throw new InvalidOperationException("Concurrency conflict occurred while restocking. Please retry.");
            }

            return updated;
        }

        public async Task<InventoryStockModel?> ReserveStockAsync(Guid productUid, int quantity)
        {
            if (quantity <= 0)
            {
                throw new InvalidOperationException("Reserve quantity must be greater than zero.");
            }

            var existing = await _inventoryRepository.FindByProduct(productUid);
            if (existing is null)
            {
                return null;
            }

            if (existing.AvailableQuantity < quantity)
            {
                throw new InvalidOperationException($"Insufficient available stock for product '{productUid}'. Available: {existing.AvailableQuantity}, Requested: {quantity}.");
            }

            existing.f_reserved += quantity;

            var updated = await _inventoryRepository.Update(existing);
            if (updated is null)
            {
                throw new InvalidOperationException("Concurrency conflict occurred while reserving stock. Please retry.");
            }

            return updated;
        }

        public async Task<InventoryStockModel?> ReleaseReservedStockAsync(Guid productUid, int quantity)
        {
            if (quantity <= 0)
            {
                throw new InvalidOperationException("Release quantity must be greater than zero.");
            }

            var existing = await _inventoryRepository.FindByProduct(productUid);
            if (existing is null)
            {
                return null;
            }

            if (existing.f_reserved < quantity)
            {
                throw new InvalidOperationException($"Cannot release {quantity} units; only {existing.f_reserved} units are currently reserved.");
            }

            existing.f_reserved -= quantity;

            var updated = await _inventoryRepository.Update(existing);
            if (updated is null)
            {
                throw new InvalidOperationException("Concurrency conflict occurred while releasing stock. Please retry.");
            }

            return updated;
        }

        public async Task<InventoryStockModel?> ConfirmDeductStockAsync(Guid productUid, int quantity)
        {
            if (quantity <= 0)
            {
                throw new InvalidOperationException("Deduction quantity must be greater than zero.");
            }

            var existing = await _inventoryRepository.FindByProduct(productUid);
            if (existing is null)
            {
                return null;
            }

            if (existing.f_reserved < quantity || existing.f_quantity < quantity)
            {
                throw new InvalidOperationException($"Cannot deduct {quantity} units; reserved ({existing.f_reserved}) or total ({existing.f_quantity}) stock is insufficient.");
            }

            existing.f_reserved -= quantity;
            existing.f_quantity -= quantity;

            var updated = await _inventoryRepository.Update(existing);
            if (updated is null)
            {
                throw new InvalidOperationException("Concurrency conflict occurred while deducting stock. Please retry.");
            }

            return updated;
        }

        public async Task<int> DeleteAsync(Guid uid)
        {
            var existing = await _inventoryRepository.Find(uid);
            if (existing is null)
            {
                return 0;
            }

            return await _inventoryRepository.Remove(existing);
        }
    }
}