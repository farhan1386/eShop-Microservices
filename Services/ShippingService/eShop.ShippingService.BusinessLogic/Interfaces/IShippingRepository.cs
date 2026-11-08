using eShop.ShippingService.Models;

namespace eShop.ShippingService.BusinessLogic.Interfaces
{
    public interface IShippingRepository
    {
        Task<IEnumerable<ShippingModel>> Get();
        Task<ShippingModel?> Find(Guid uid);
        Task<ShippingModel?> FindByOrderUid(Guid orderUid);
        Task<bool> IsOrderTracked(Guid orderUid);
        Task<ShippingModel> Add(ShippingModel model);
        Task<ShippingModel> Update(ShippingModel model);
        Task<int> Remove(ShippingModel model);

        Task<IEnumerable<ShippingHistoryModel>> GetHistory(Guid shippingManifestUid);
        Task<ShippingHistoryModel> AddHistory(ShippingHistoryModel model);
    }
}
