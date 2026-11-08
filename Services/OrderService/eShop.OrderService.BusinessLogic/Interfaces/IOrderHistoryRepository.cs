using eShop.OrderService.Models;

namespace eShop.OrderService.BusinessLogic.Interfaces
{
    public interface IOrderHistoryRepository
    {
        Task<IEnumerable<OrderHistoryModel>> GetByOrder(Guid orderUid);
        Task<OrderHistoryModel> Add(OrderHistoryModel model);
    }
}