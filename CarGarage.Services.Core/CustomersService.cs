using CarGarage.Data;
using CarGarage.DataModels;
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
                    (c is LegalEntityCustomer && ((LegalEntityCustomer)c).CompanyName.ToLower().Contains(searchTerm)));
            }

            var customers = await query
                .Select(c => new CustomerInfoViewModel
                {
                    Id = c.Id,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    City = c.City,
                    CarsCount = c.Cars.Count,
                    DisplayName = c is IndividualCustomer
                        ? ((IndividualCustomer)c).FirstName + " " + ((IndividualCustomer)c).LastName
                        : ((LegalEntityCustomer)c).CompanyName,
                    CustomerType = c is IndividualCustomer ? "Физическо лице" : "Юридическо лице",
                    UniqueNumber = c is IndividualCustomer ? ((IndividualCustomer)c).Egn : ((LegalEntityCustomer)c).VatNumber
                }).ToListAsync();

            return new CustomerIndexViewModel { Customers = customers, SearchTerm = searchTerm };
        }

        public async Task<CustomerDetailsViewModel?> GetCustomerDetailsAsync(int id)
        {
            var customer = await context.Customers
                .Include(c => c.Cars)
                    .ThenInclude(car => car.Invoices)
                        .ThenInclude(inv => inv.Parts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null) return null;

            var model = new CustomerDetailsViewModel
            {
                Id = customer.Id,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                City = customer.City,
                DisplayName = customer is IndividualCustomer
                    ? ((IndividualCustomer)customer).FirstName + " " + ((IndividualCustomer)customer).LastName
                    : ((LegalEntityCustomer)customer).CompanyName,
                CustomerType = customer is IndividualCustomer ? "Физическо лице" : "Юридическо лице",
                UniqueNumber = customer is IndividualCustomer ? ((IndividualCustomer)customer).Egn : ((LegalEntityCustomer)customer).VatNumber,

                Cars = customer.Cars.Select(car => new CustomerCarViewModel
                {
                    Id = car.Id,
                    Make = car.Make,
                    Model = car.Model,
                    RegistrationNumber = car.RegistrationNumber,
                    ImageUrl = car.ImageUrl,
                    InvoicesCount = car.Invoices.Count
                }).ToList(),

                RepairHistory = customer.Cars
                    .SelectMany(car => car.Invoices.Select(inv => new CustomerRepairHistoryViewModel
                    {
                        InvoiceId = inv.Id, 
                        CarModel = $"{car.Make} {car.Model}",
                        RegistrationNumber = car.RegistrationNumber,
                        Date = inv.IssuedDate,
                        InvoiceNumber = inv.InvoiceNumber,
                        Description = inv.Notes, //туй май ша го махаммм
                       
                        TotalAmount = inv.TotalLaborPrice + inv.Parts.Sum(p => p.TotalPrice)
                    }))
                    .OrderByDescending(r => r.Date)
                    .ToList()
            };

            return model;
        }
    }
}