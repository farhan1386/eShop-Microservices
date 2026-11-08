using eShop.ShippingService.BusinessLogic.Infrastructure;
using eShop.ShippingService.BusinessLogic.Interfaces;
using eShop.ShippingService.Models;

namespace eShop.ShippingService.BusinessLogic.Services
{
    public class ShippingService : IShippingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IShippingRepository _shippingRepository;

        public ShippingService(
            IUnitOfWork unitOfWork,
            IShippingRepository shippingRepository)
        {
            _unitOfWork = unitOfWork;
            _shippingRepository = shippingRepository;
        }

        public Task<IEnumerable<ShippingModel>> GetAllManifestsAsync()
        {
            return _shippingRepository.Get();
        }

        public Task<ShippingModel?> GetManifestAsync(Guid shippingManifestUid)
        {
            return _shippingRepository.Find(shippingManifestUid);
        }

        public Task<ShippingModel?> GetManifestByOrderAsync(Guid orderUid)
        {
            return _shippingRepository.FindByOrderUid(orderUid);
        }

        public async Task<bool> InitiateShippingAsync(ShippingModel manifest, ShippingHistoryModel initialHistory)
        {
            var isTracked = await _shippingRepository.IsOrderTracked(manifest.f_order_uid);
            if (isTracked)
            {
                throw new InvalidOperationException($"The order '{manifest.f_order_uid}' already has a shipping manifest setup.");
            }

            await _unitOfWork.BeginAsync();

            try
            {
                var createdManifest = await _shippingRepository.Add(manifest);

                initialHistory.f_shipping_manifest_uid = createdManifest.f_uid;
                await _shippingRepository.AddHistory(initialHistory);

                await _unitOfWork.CommitAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateTrackingStateAsync(Guid shippingManifestUid, string newStatus, string? location, string? comment, string? trackingNumber, DateTime? estimatedDelivery)
        {
            var manifest = await _shippingRepository.Find(shippingManifestUid);
            if (manifest is null)
            {
                return false;
            }

            string oldStatus = manifest.f_shipping_status;

            manifest.f_shipping_status = newStatus;

            if (trackingNumber is not null)
            {
                manifest.f_tracking_number = trackingNumber;
            }

            if (estimatedDelivery is not null)
            {
                manifest.f_estimated_delivery = estimatedDelivery;
            }

            await _unitOfWork.BeginAsync();

            try
            {
                await _shippingRepository.Update(manifest);

                var history = new ShippingHistoryModel
                {
                    f_shipping_manifest_uid = shippingManifestUid,
                    f_old_status = oldStatus,
                    f_new_status = newStatus,
                    f_status_location = location,
                    f_comment = comment
                };

                await _shippingRepository.AddHistory(history);

                await _unitOfWork.CommitAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public Task<IEnumerable<ShippingHistoryModel>> GetTrackingHistoryAsync(Guid shippingManifestUid)
        {
            return _shippingRepository.GetHistory(shippingManifestUid);
        }
    }
}