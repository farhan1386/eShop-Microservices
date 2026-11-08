using eShop.OrderService.Models;

namespace eShop.OrderService.BusinessLogic.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderModel>> Get();
        Task<OrderModel?> Find(Guid uid);
        Task<IEnumerable<OrderModel>> GetByCustomer(Guid customerUid);
        Task<OrderModel> Add(OrderModel model);
        Task<OrderModel> Update(OrderModel model);
        Task<int> Remove(OrderModel model);
    }
}