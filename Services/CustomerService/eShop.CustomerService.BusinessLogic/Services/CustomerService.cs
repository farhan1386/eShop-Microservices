using eShop.CustomerService.BusinessLogic.Infrastructure;
using eShop.CustomerService.BusinessLogic.Interfaces;
using eShop.CustomerService.Models;
using System.Security.Cryptography;
using System.Text;

namespace eShop.CustomerService.BusinessLogic.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerAddressRepository _addressRepository;

        public CustomerService(
            IUnitOfWork unitOfWork,
            ICustomerRepository customerRepository,
            ICustomerAddressRepository addressRepository)
        {
            _unitOfWork = unitOfWork;
            _customerRepository = customerRepository;
            _addressRepository = addressRepository;
        }

        public Task<IEnumerable<CustomerModel>> GetAllAsync()
        {
            return _customerRepository.Get();
        }

        public Task<CustomerModel?> GetProfileAsync(Guid customerUid)
        {
            return _customerRepository.Find(customerUid);
        }

        public async Task<bool> OnboardNewCustomerAsync(CustomerModel customer, CustomerAddressModel primaryAddress)
        {
            var isAvailable = await _customerRepository.IsEmailAvailable(customer.f_email);
            if (!isAvailable)
            {
                throw new InvalidOperationException($"The email address '{customer.f_email}' is already registered.");
            }

            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(customer.f_password));
                customer.f_password = Convert.ToBase64String(hashedBytes);
            }

            await _unitOfWork.BeginAsync();

            try
            {
                var createdCustomer = await _customerRepository.Add(customer);

                primaryAddress.f_customer_uid = createdCustomer.f_uid;
                await _addressRepository.Add(primaryAddress);

                await _unitOfWork.CommitAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ModifyContactDetailsAsync(Guid customerUid, string firstName, string lastName, string phone)
        {
            var profile = await _customerRepository.Find(customerUid);
            if (profile is null)
            {
                return false;
            }

            profile.f_fname = firstName;
            profile.f_lname = lastName;
            profile.f_phone = phone;

            await _customerRepository.Update(profile);
            return true;
        }
    }
}