using CarGarage.ViewModels.Customers;

namespace CarGarage.Services.Core.Contracts
{
    public interface ICustomersService
    {
        Task<CustomerIndexViewModel> GetAllCustomersAsync(string? searchTerm);
    }
}