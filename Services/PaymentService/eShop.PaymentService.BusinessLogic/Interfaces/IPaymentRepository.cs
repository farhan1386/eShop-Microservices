using eShop.PaymentService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace eShop.PaymentService.BusinessLogic.Interfaces
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<PaymentModel>> Get();
        Task<PaymentModel?> Find(Guid uid);
        Task<IEnumerable<PaymentModel>> GetByOrder(Guid orderUid);
        Task<PaymentModel> Add(PaymentModel model);
        Task<PaymentModel> Update(PaymentModel model);
        Task<int> Remove(PaymentModel model);
    }
}
