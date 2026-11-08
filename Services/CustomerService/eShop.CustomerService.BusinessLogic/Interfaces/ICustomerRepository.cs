using eShop.CustomerService.Models;

namespace eShop.CustomerService.BusinessLogic.Interfaces
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<CustomerModel>> Get();
        Task<CustomerModel?> Find(Guid uid);
        Task<CustomerModel?> FindByEmail(string email);
        Task<bool> IsEmailAvailable(string email);
        Task<CustomerModel> Add(CustomerModel model);
        Task<CustomerModel> Update(CustomerModel model);
        Task<bool> UpdatePassword(Guid uid, string passwordHash, Guid operatorUid);
        Task<bool> UpdateLastLogin(Guid uid, DateTime timestamp);
        Task<int> Remove(CustomerModel model);
    }
}
