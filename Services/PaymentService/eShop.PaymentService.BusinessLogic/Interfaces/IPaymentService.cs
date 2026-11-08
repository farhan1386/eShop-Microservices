using eShop.Contracts.Enums;
using eShop.PaymentService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.PaymentService.BusinessLogic.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentModel>> GetAllAsync();
        Task<PaymentModel?> GetByUidAsync(Guid uid);
        Task<IEnumerable<PaymentModel>> GetByOrderAsync(Guid orderUid);
        Task<PaymentModel> InitiatePaymentAsync(PaymentModel payment);
        Task<PaymentModel?> CompletePaymentAsync(Guid uid, string gatewayTxId);
        Task<PaymentModel?> UpdateStatusAsync(Guid uid, PaymentStatus status, string? gatewayTxId = null);
        Task<int> DeleteAsync(Guid uid);
    }
}
