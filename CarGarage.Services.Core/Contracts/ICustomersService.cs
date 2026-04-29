using CarGarage.ViewModels.Customers;

namespace CarGarage.Services.Core.Contracts
{
    public interface ICustomersService
    {
        Task<CustomerIndexViewModel> GetAllCustomersAsync(string? searchTerm);

        Task<CustomerDetailsViewModel?> GetCustomerDetailsAsync(int id);


        Task SaveCustomerAsync(CustomerFormViewModel model);
        Task<CustomerFormViewModel?> GetCustomerForEditAsync(int id);
    }
}