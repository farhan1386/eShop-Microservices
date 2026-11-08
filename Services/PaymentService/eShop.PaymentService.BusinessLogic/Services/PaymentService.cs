using eShop.Contracts.Enums;
using eShop.PaymentService.BusinessLogic.Interfaces;
using eShop.PaymentService.Models;

namespace eShop.PaymentService.BusinessLogic.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public Task<IEnumerable<PaymentModel>> GetAllAsync()
        {
            return _paymentRepository.Get();
        }

        public Task<PaymentModel?> GetByUidAsync(Guid uid)
        {
            return _paymentRepository.Find(uid);
        }

        public Task<IEnumerable<PaymentModel>> GetByOrderAsync(Guid orderUid)
        {
            return _paymentRepository.GetByOrder(orderUid);
        }

        public async Task<PaymentModel> InitiatePaymentAsync(PaymentModel payment)
        {
            if (payment.f_amount <= 0)
            {
                throw new InvalidOperationException("Payment amount must be greater than zero.");
            }

            payment.f_status = PaymentStatus.Pending;
            return await _paymentRepository.Add(payment);
        }

        public async Task<PaymentModel?> CompletePaymentAsync(Guid uid, string gatewayTxId)
        {
            var existing = await _paymentRepository.Find(uid);
            if (existing is null)
            {
                return null;
            }

            existing.f_status = PaymentStatus.Completed;
            existing.f_gateway_txid = gatewayTxId;
            existing.f_payment_date = DateTime.UtcNow;

            return await _paymentRepository.Update(existing);
        }

        public async Task<PaymentModel?> UpdateStatusAsync(Guid uid, PaymentStatus status, string? gatewayTxId = null)
        {
            var existing = await _paymentRepository.Find(uid);
            if (existing is null)
            {
                return null;
            }

            existing.f_status = status;
            if (!string.IsNullOrWhiteSpace(gatewayTxId))
            {
                existing.f_gateway_txid = gatewayTxId;
            }
            if (status == PaymentStatus.Completed && existing.f_payment_date is null)
            {
                existing.f_payment_date = DateTime.UtcNow;
            }

            return await _paymentRepository.Update(existing);
        }

        public async Task<int> DeleteAsync(Guid uid)
        {
            var existing = await _paymentRepository.Find(uid);
            if (existing is null)
            {
                return 0;
            }

            return await _paymentRepository.Remove(existing);
        }
    }
}