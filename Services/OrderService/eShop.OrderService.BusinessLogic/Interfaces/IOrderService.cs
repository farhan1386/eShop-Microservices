using eShop.Contracts.Enums;
using eShop.OrderService.Models;

namespace eShop.OrderService.BusinessLogic.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderModel>> GetAllAsync();
        Task<OrderModel?> GetByUidAsync(Guid uid);
        Task<IEnumerable<OrderModel>> GetByCustomerAsync(Guid customerUid);
        Task<IEnumerable<OrderItemModel>> GetItemsByOrderAsync(Guid orderUid);
        Task<IEnumerable<OrderHistoryModel>> GetHistoryByOrderAsync(Guid orderUid);
        Task<OrderModel> PlaceOrderAsync(OrderModel order, IEnumerable<OrderItemModel> items);
        Task<OrderModel?> UpdateStatusAsync(Guid uid, OrderStatus newStatus, string? comment = null);
        Task<int> CancelOrderAsync(Guid uid, string? comment = null);
    }
}
