using CarGarage.Data;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Customers;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Services.Core
{
    public class CustomersService(ApplicationDbContext context) : ICustomersService
    {
        public async Task<CustomerIndexViewModel> GetAllCustomersAsync(string? searchTerm)
        {
            var query = context.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                
                query = query.Where(c =>
                    c.Email.ToLower().Contains(searchTerm) ||
                    c.PhoneNumber.Contains(searchTerm) ||
                    (c is IndividualCustomer && ((IndividualCustomer)c).FirstName.ToLower().Contains(searchTerm)) ||
                    (c is IndividualCustomer && ((IndividualCustomer)c).LastName.ToLower().Contains(searchTerm)) ||
                    (c is LegalEntityCustomer && ((LegalEntityCustomer)c).CompanyName.ToLower().Contains(searchTerm))
                );
            }

            var customers = await query
                .Select(c => new CustomerInfoViewModel
                {
                    Id = c.Id,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    City = c.City,
                    CarsCount = c.Cars.Count,
                    // Проверявам типа на клиента за да извлекаа правилното име и ном.ер
                    DisplayName = c is IndividualCustomer
                        ? ((IndividualCustomer)c).FirstName + " " + ((IndividualCustomer)c).LastName
                        : ((LegalEntityCustomer)c).CompanyName,
                    CustomerType = c is IndividualCustomer ? "Физическо лице" : "Юридическо лице",
                    UniqueNumber = c is IndividualCustomer
                        ? ((IndividualCustomer)c).Egn
                        : ((LegalEntityCustomer)c).VatNumber
                })
                .ToListAsync();

            return new CustomerIndexViewModel
            {
                SearchTerm = searchTerm,
                Customers = customers
            };
        }
    }
}