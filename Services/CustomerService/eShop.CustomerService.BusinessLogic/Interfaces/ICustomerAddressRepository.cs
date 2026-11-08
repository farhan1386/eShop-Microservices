using eShop.CustomerService.Models;

namespace eShop.CustomerService.BusinessLogic.Interfaces
{
    public interface ICustomerAddressRepository
    {
        Task<IEnumerable<CustomerAddressModel>> GetByCustomer(Guid customerUid);
        Task<CustomerAddressModel?> Find(Guid uid, Guid customerUid);
        Task<CustomerAddressModel> Add(CustomerAddressModel model);
        Task<CustomerAddressModel> Update(CustomerAddressModel model);
        Task<bool> SetDefault(Guid customerUid, Guid addressUid, Guid operatorUid);
        Task<int> Remove(CustomerAddressModel model);
    }
}
