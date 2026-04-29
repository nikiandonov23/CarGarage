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


        public async Task<CustomerFormViewModel?> GetCustomerForEditAsync(int id)
        {
            var c = await context.Customers.FirstOrDefaultAsync(x => x.Id == id);
            if (c == null) return null;

            var model = new CustomerFormViewModel
            {
                Id = c.Id,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                Address = c.Address,
                City = c.City,
                GarageId = c.GarageId,
                CustomerType = c is IndividualCustomer ? "Individual" : "Legal"
            };

            if (c is IndividualCustomer ind)
            {
                model.FirstName = ind.FirstName;
                model.LastName = ind.LastName;
                model.Egn = ind.Egn;
            }
            else if (c is LegalEntityCustomer leg)
            {
                model.CompanyName = leg.CompanyName;
                model.VatNumber = leg.VatNumber;
                model.IsVatRegistered = leg.IsVatRegistered;
                model.ResponsiblePerson = leg.ResponsiblePerson;
            }

            return model;
        }

        public async Task SaveCustomerAsync(CustomerFormViewModel model)
        {
            Customer entity;

            if (model.Id.HasValue && model.Id > 0)
            {
                // Edit логика - намираме съществуващия
                entity = await context.Customers.FirstAsync(c => c.Id == model.Id);
                // Тук се обновяват общите полета
                entity.Email = model.Email;
                entity.PhoneNumber = model.PhoneNumber;
                entity.Address = model.Address;
                entity.City = model.City;

                // ВАЖНО: Тук се обновяват специфичните полета според типа
                if (entity is IndividualCustomer ind)
                {
                    ind.FirstName = model.FirstName!;
                    ind.LastName = model.LastName!;
                    ind.Egn = model.Egn!;
                }
                else if (entity is LegalEntityCustomer leg)
                {
                    leg.CompanyName = model.CompanyName!;
                    leg.VatNumber = model.VatNumber!;
                    leg.IsVatRegistered = model.IsVatRegistered;
                    leg.ResponsiblePerson = model.ResponsiblePerson!;
                }
            }
            else
            {
                // Create логика - инстанцираме правилния клас
                if (model.CustomerType == "Individual")
                {
                    entity = new IndividualCustomer
                    {
                        FirstName = model.FirstName!,
                        LastName = model.LastName!,
                        Egn = model.Egn!
                    };
                }
                else
                {
                    entity = new LegalEntityCustomer
                    {
                        CompanyName = model.CompanyName!,
                        VatNumber = model.VatNumber!,
                        IsVatRegistered = model.IsVatRegistered,
                        ResponsiblePerson = model.ResponsiblePerson!
                    };
                }

                entity.Email = model.Email;
                entity.PhoneNumber = model.PhoneNumber;
                entity.Address = model.Address;
                entity.City = model.City;
                entity.GarageId = 1; // Замени с текущия GarageId на логнатия потребител

                await context.Customers.AddAsync(entity);
            }

            await context.SaveChangesAsync();
        }
    }
}