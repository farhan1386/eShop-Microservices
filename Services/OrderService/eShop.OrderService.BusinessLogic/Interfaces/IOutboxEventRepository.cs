using eShop.OrderService.Models;

namespace eShop.OrderService.BusinessLogic.Interfaces
{
    public interface IOutboxEventRepository
    {
        Task<OutboxEventModel> Add(OutboxEventModel model);
        Task<IEnumerable<OutboxEventModel>> GetPending(int batchSize);
        Task<bool> UpdateStatus(Guid uid, string status, DateTime? processedDate, string? errorLog);
    }
}
