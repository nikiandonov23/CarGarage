using CarGarage.Data;
using CarGarage.DataModels;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Customers;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Services.Core
{
    public class CustomersService(ApplicationDbContext context) : ICustomersService
    {
        public async Task<CustomerIndexViewModel> GetAllCustomersAsync(string? searchTerm, string userId)
        {
            // Филтрираме клиентите, така че потребителят да вижда само тези от неговия гараж
            var query = context.Customers
                .Where(c => c.Garage != null && c.Garage.OwnerId == userId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(c =>
                    (c.Email ?? string.Empty).ToLower().Contains(searchTerm) ||
                    (c.PhoneNumber ?? string.Empty).Contains(searchTerm) ||
                    (c is IndividualCustomer && (((IndividualCustomer)c).FirstName ?? string.Empty).ToLower().Contains(searchTerm)) ||
                    (c is IndividualCustomer && (((IndividualCustomer)c).LastName ?? string.Empty).ToLower().Contains(searchTerm)) ||
                    (c is LegalEntityCustomer && (((LegalEntityCustomer)c).CompanyName ?? string.Empty).ToLower().Contains(searchTerm)));
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

        public async Task<CustomerDetailsViewModel?> GetCustomerDetailsAsync(int id, string userId)
        {
            // Проверка дали клиентът принадлежи на гаража на текущия потребител
            var customer = await context.Customers
                .Include(c => c.Cars)
                    .ThenInclude(car => car.Invoices)
                        .ThenInclude(inv => inv.Parts)
                .FirstOrDefaultAsync(c => c.Id == id && c.Garage != null && c.Garage.OwnerId == userId);

            if (customer == null) return null;

            return new CustomerDetailsViewModel
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
                        Description = inv.Notes,
                        TotalAmount = inv.TotalLaborPrice + inv.Parts.Sum(p => p.TotalPrice)
                    }))
                    .OrderByDescending(r => r.Date)
                    .ToList()
            };
        }

        public async Task<CustomerFormViewModel?> GetCustomerForEditAsync(int id, string userId)
        {
            var c = await context.Customers
                .FirstOrDefaultAsync(x => x.Id == id && x.Garage != null && x.Garage.OwnerId == userId);

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

        public async Task SaveCustomerAsync(CustomerFormViewModel model, string userId)
        {
            // 1. Намираме гаража на потребителя
            var garageId = await context.Garages
                .Where(g => g.OwnerId == userId)
                .Select(g => g.Id)
                .FirstOrDefaultAsync();

            if (garageId == 0) throw new InvalidOperationException("Потребителят няма регистриран гараж.");

            Customer? entity;

            if (model.Id.HasValue && model.Id > 0)
            {
                // EDIT: Намираме съществуващия клиент, като се уверяваме, че е от същия гараж
                entity = await context.Customers
                    .FirstOrDefaultAsync(c => c.Id == model.Id && c.GarageId == garageId);

                if (entity == null) throw new UnauthorizedAccessException("Нямате достъп до този клиент.");
            }
            else
            {
                // CREATE: Инстанцираме правилния тип
                if (model.CustomerType == "Individual")
                {
                    entity = new IndividualCustomer();
                }
                else
                {
                    entity = new LegalEntityCustomer();
                }

                entity.GarageId = garageId;
                await context.Customers.AddAsync(entity);
            }

            // Общи полета
            entity.Email = model.Email;
            entity.PhoneNumber = model.PhoneNumber;
            entity.Address = model.Address;
            entity.City = model.City;

            // Специфични полета
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

            await context.SaveChangesAsync();
        }
    }
}