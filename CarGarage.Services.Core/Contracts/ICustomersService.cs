using CarGarage.ViewModels.Customers;

namespace CarGarage.Services.Core.Contracts
{
    public interface ICustomersService
    {
        Task<CustomerIndexViewModel> GetAllCustomersAsync(string? searchTerm, string userId);

        Task<CustomerDetailsViewModel?> GetCustomerDetailsAsync(int id, string userId);


        Task SaveCustomerAsync(CustomerFormViewModel model, string userId);
        Task<CustomerFormViewModel?> GetCustomerForEditAsync(int id, string userId);
    }
}