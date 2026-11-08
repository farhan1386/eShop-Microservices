using eShop.CustomerService.Models;

namespace eShop.CustomerService.BusinessLogic.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerModel>> GetAllAsync();
        Task<CustomerModel?> GetProfileAsync(Guid customerUid);
        Task<bool> OnboardNewCustomerAsync(CustomerModel customer, CustomerAddressModel primaryAddress);
        Task<bool> ModifyContactDetailsAsync(Guid customerUid, string firstName, string lastName, string phone);
    }
}