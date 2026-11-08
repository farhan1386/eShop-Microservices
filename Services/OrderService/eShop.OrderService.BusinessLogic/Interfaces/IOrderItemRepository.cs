using eShop.OrderService.Models;

namespace eShop.OrderService.BusinessLogic.Interfaces
{
    public interface IOrderItemRepository
    {
        Task<IEnumerable<OrderItemModel>> GetByOrder(Guid orderUid);
        Task<OrderItemModel?> Find(Guid uid);
        Task<OrderItemModel> Add(OrderItemModel model);
        Task<OrderItemModel> Update(OrderItemModel model);
        Task<int> Remove(OrderItemModel model);
    }
}