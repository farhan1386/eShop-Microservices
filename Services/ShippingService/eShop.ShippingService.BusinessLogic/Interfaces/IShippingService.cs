using eShop.ShippingService.Models;

namespace eShop.ShippingService.BusinessLogic.Interfaces
{
    public interface IShippingService
    {
        Task<IEnumerable<ShippingModel>> GetAllManifestsAsync();
        Task<ShippingModel?> GetManifestAsync(Guid shippingManifestUid);
        Task<ShippingModel?> GetManifestByOrderAsync(Guid orderUid);
        Task<bool> InitiateShippingAsync(ShippingModel manifest, ShippingHistoryModel initialHistory);
        Task<bool> UpdateTrackingStateAsync(Guid shippingManifestUid, string newStatus, string? location, string? comment, string? trackingNumber, DateTime? estimatedDelivery);
        Task<IEnumerable<ShippingHistoryModel>> GetTrackingHistoryAsync(Guid shippingManifestUid);
    }
}
