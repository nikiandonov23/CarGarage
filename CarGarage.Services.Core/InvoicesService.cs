using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarGarage.Data;
using CarGarage.DataModels;
using CarGarage.DataModels.Enums;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Invoices;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Services.Core
{
    public class InvoicesService(ApplicationDbContext context) : IInvoicesService
    {
        public async Task<InvoiceFormModel> GetNewInvoiceModelAsync(int carId, string userId)
        {
            var car = await context.Cars
                .Include(c => c.Customer)
                    .ThenInclude(cust => cust.Garage)
                .FirstOrDefaultAsync(c => c.Id == carId && !c.IsDeleted && (
                    (c.Customer != null && c.Customer.Garage != null && c.Customer.Garage.OwnerId == userId)
                    || c.UserCars.Any(uc => uc.UserId == userId)
                ));

            if (car == null) throw new Exception("Car not found or access denied");

            var parts = await context.Parts
                .Where(p => p.CarId == carId &&
                            p.InvoiceId == null &&
                            ((p.Car.Customer != null && p.Car.Customer.Garage != null && p.Car.Customer.Garage.OwnerId == userId)
                              || p.Car.UserCars.Any(uc => uc.UserId == userId)))
                .ToListAsync();

            return new InvoiceFormModel
            {
                CarId = carId,
                CarInfo = $"{car.Make} {car.Model} ({car.RegistrationNumber})",
                AvailableParts = parts.Select(p => new PartSelectionViewModel
                {
                    PartId = p.Id,
                    Description = p.Description,
                    TotalPrice = p.TotalPrice,
                    IsSelected = true
                }).ToList()
            };
        }

        public async Task<int> CreateInvoiceAsync(InvoiceFormModel model, string userId)
        {
            var carData = await context.Cars
                .Where(c => c.Id == model.CarId && !c.IsDeleted && (
                    (c.Customer != null && c.Customer.Garage != null && c.Customer.Garage.OwnerId == userId)
                    || c.UserCars.Any(uc => uc.UserId == userId)
                ))
                .Select(c => new { GarageId = c.Customer != null ? (int?)c.Customer.GarageId : null })
                .FirstOrDefaultAsync();

            if (carData == null)
            {
                throw new InvalidOperationException("Garage not found or access denied for this car.");
            }

            var garageId = carData.GarageId;
            if (!garageId.HasValue)
            {
                garageId = await context.Garages
                    .Where(g => g.OwnerId == userId)
                    .Select(g => (int?)g.Id)
                    .FirstOrDefaultAsync();
            }

            if (!garageId.HasValue || garageId.Value == 0)
            {
                throw new InvalidOperationException("Garage not found or access denied for this car.");
            }

            // --- СЧЕТОВОДНО ГЕНЕРИРАНЕ НА НОМЕР ---
            // 1. Намираме всички номера на фактури за този гараж
            var existingInvoiceNumbers = await context.Invoices
                .Where(i => i.GarageId == garageId.Value)
                .Select(i => i.InvoiceNumber)
                .ToListAsync();

            // 2. Преобразуваме ги в числа и намираме най-голямото
            long maxNumber = 0;
            foreach (var numStr in existingInvoiceNumbers)
            {
                if (long.TryParse(numStr, out long currentNum))
                {
                    if (currentNum > maxNumber)
                    {
                        maxNumber = currentNum;
                    }
                }
            }

            // 3. Увеличаваме с 1 (ако няма издадени, първата фактура ще бъде 1)
            long nextNumber = maxNumber + 1;

            // 4. Форматираме до 10 цифри с водещи нули (D10)
            string formattedInvoiceNumber = nextNumber.ToString("D10");
            // -------------------------------------

            var invoice = new Invoice
            {
                CarId = model.CarId,
                GarageId = garageId.Value,
                IssuedDate = DateTime.UtcNow,
                InvoiceNumber = formattedInvoiceNumber,
                PaymentMethod = model.PaymentMethod, // <-- ЗАПИСВАМЕ ИЗБРАНИЯ МЕТОД
                LaborPricePerHour = model.LaborPricePerHour,
                LaborHours = model.LaborHours,
                TaxPercentage = model.TaxPercentage,
                Notes = model.Notes
            };

            var selectedIds = model.AvailableParts
                .Where(x => x.IsSelected)
                .Select(x => x.PartId)
                .ToList();

            var parts = await context.Parts
                .Where(p => selectedIds.Contains(p.Id) && (
                    (p.Car.Customer != null && p.Car.Customer.Garage != null && p.Car.Customer.Garage.OwnerId == userId)
                    || p.Car.UserCars.Any(uc => uc.UserId == userId)
                ))
                .ToListAsync();

            foreach (var part in parts)
            {
                invoice.Parts.Add(part);
            }

            await context.Invoices.AddAsync(invoice);
            await context.SaveChangesAsync();

            return invoice.Id;
        }

        public async Task<InvoiceFullViewModel> GetInvoiceDetailsAsync(int invoiceId, string userId)
        {
            var inv = await context.Invoices
                .Include(i => i.Car)
                    .ThenInclude(c => c.Customer)
                .Include(i => i.Parts)
                .Include(i => i.Garage)
                .FirstOrDefaultAsync(i => i.Id == invoiceId && i.Garage != null && i.Garage.OwnerId == userId);

            if (inv == null) throw new Exception("Invoice not found or access denied");

            decimal subTotalParts = inv.Parts.Sum(p => p.TotalPrice);
            decimal subTotalLabor = (decimal)inv.LaborHours * inv.LaborPricePerHour;
            decimal totalBeforeTax = subTotalParts + subTotalLabor;
            bool isVat = inv.Garage?.IsVatRegistered ?? false;
            decimal taxAmount = isVat ? (totalBeforeTax * (inv.TaxPercentage / 100)) : 0m;

            string? clientName = null;
            string? clientIdNumber = null;
            string? clientAddress = null;
            string? clientType = null;
            string? clientVatNumber = null;
            string? clientMOL = null;
            string? clientEmail = null;
            string? clientPhone = null;

            if (inv.Car.Customer != null)
            {
                var cust = inv.Car.Customer;
                clientAddress = $"гр. {cust.City}, {cust.Address}";
                clientEmail = cust.Email;
                clientPhone = cust.PhoneNumber;

                if (cust is IndividualCustomer ind)
                {
                    clientName = $"{ind.FirstName} {ind.LastName}";
                    clientIdNumber = ind.Egn;
                    clientType = "Физическо лице";
                }
                else if (cust is LegalEntityCustomer org)
                {
                    clientName = org.CompanyName;
                    clientIdNumber = org.VatNumber;
                    clientType = "Юридическо лице";
                    clientMOL = org.ResponsiblePerson;
                    if (org.IsVatRegistered)
                    {
                        clientVatNumber = org.VatNumber.StartsWith("BG") ? org.VatNumber : "BG" + org.VatNumber;
                    }
                }
            }

            return new InvoiceFullViewModel
            {
                Id = invoiceId,
                InvoiceNumber = inv.InvoiceNumber,
                IssuedDate = inv.IssuedDate,
                IsCancelled = inv.IsCancelled,

                // --- МАПВАНЕ НА ПЛАЩАНЕТО ---
                PaymentMethod = inv.PaymentMethod,
                PaymentMethodText = GetPaymentMethodDisplayName(inv.PaymentMethod),

                CarInfo = $"{inv.Car.Make} {inv.Car.Model}",
                Vin = inv.Car.Vin,
                RegNumber = inv.Car.RegistrationNumber,

                // --- МАПВАНЕ НА КЛИЕНТ ДАННИ ---
                ClientName = clientName,
                ClientIdNumber = clientIdNumber,
                ClientAddress = clientAddress,
                ClientType = clientType,
                ClientVatNumber = clientVatNumber,
                ClientMOL = clientMOL,
                ClientEmail = clientEmail,
                ClientPhone = clientPhone,

                Parts = inv.Parts.Select(p => new InvoicePartViewModel
                {
                    Name = p.Description,
                    Price = p.TotalPrice
                }).ToList(),
                LaborTotal = subTotalLabor,
                TaxAmount = taxAmount,
                GrandTotal = totalBeforeTax + taxAmount,
                Notes = inv.Notes,

                // --- МАПВАНЕ НА ДАННИТЕ ЗА СЕРВИЗА ---
                GarageName = inv.Garage?.Name ?? "Сервиз",
                GarageBulstat = inv.Garage?.Bulstat ?? "-",
                GarageOwnerName = inv.Garage?.OwnerName,
                GarageCity = inv.Garage?.City ?? "-",
                GarageAddress = inv.Garage?.Address ?? "-",
                GaragePhoneNumber = inv.Garage?.PhoneNumber,
                GarageIsVatRegistered = isVat,
                GarageIBAN = inv.Garage?.IBAN,
                GarageBIC = inv.Garage?.BIC,
                GarageBankName = inv.Garage?.BankName
            };
        }

        public async Task<IEnumerable<InvoiceFullViewModel>> GetAllUserInvoicesAsync(
            string userId,
            string? status = null,
            PaymentMethod? paymentMethod = null,
            string? clientSearch = null,
            string? carSearch = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? sortBy = null,
            bool? isAsc = null)
        {
            var query = context.Invoices
                .Where(i => i.Garage != null && i.Garage.OwnerId == userId)
                .AsQueryable();

            // 1. Филтър по статус: Активни / Анулирани
            if (!string.IsNullOrEmpty(status))
            {
                if (status.Equals("active", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => !i.IsCancelled);
                }
                else if (status.Equals("cancelled", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(i => i.IsCancelled);
                }
            }

            // 2. Филтър по начин на плащане
            if (paymentMethod.HasValue)
            {
                query = query.Where(i => i.PaymentMethod == paymentMethod.Value);
            }

            // 3. Търсене по клиент (име на клиент или ЕИК/ЕГН)
            if (!string.IsNullOrEmpty(clientSearch))
            {
                clientSearch = clientSearch.Trim().ToLower();
                query = query.Where(i => i.Car.Customer != null && (
                    (i.Car.Customer is IndividualCustomer && 
                     ((((IndividualCustomer)i.Car.Customer).FirstName ?? string.Empty).ToLower().Contains(clientSearch) ||
                      (((IndividualCustomer)i.Car.Customer).LastName ?? string.Empty).ToLower().Contains(clientSearch) ||
                      ((((IndividualCustomer)i.Car.Customer).FirstName ?? string.Empty) + " " + (((IndividualCustomer)i.Car.Customer).LastName ?? string.Empty)).ToLower().Contains(clientSearch) ||
                      (((IndividualCustomer)i.Car.Customer).Egn ?? string.Empty).Contains(clientSearch))) ||
                    (i.Car.Customer is LegalEntityCustomer && 
                     ((((LegalEntityCustomer)i.Car.Customer).CompanyName ?? string.Empty).ToLower().Contains(clientSearch) ||
                      (((LegalEntityCustomer)i.Car.Customer).VatNumber ?? string.Empty).Contains(clientSearch)))
                ));
            }

            // 4. Търсене по автомобил / Регистрационен номер / VIN
            if (!string.IsNullOrEmpty(carSearch))
            {
                carSearch = carSearch.Trim().ToLower();
                query = query.Where(i => 
                    (i.Car.RegistrationNumber ?? string.Empty).ToLower().Contains(carSearch) ||
                    (i.Car.Vin ?? string.Empty).ToLower().Contains(carSearch) ||
                    (i.Car.Make ?? string.Empty).ToLower().Contains(carSearch) ||
                    (i.Car.Model ?? string.Empty).ToLower().Contains(carSearch) ||
                    ((i.Car.Make ?? string.Empty) + " " + (i.Car.Model ?? string.Empty)).ToLower().Contains(carSearch)
                );
            }

            // 5. Филтър по период: startDate и endDate
            if (startDate.HasValue)
            {
                query = query.Where(i => i.IssuedDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(i => i.IssuedDate <= endOfDay);
            }

            var invoices = await query
                .Include(i => i.Car)
                .Include(i => i.Parts)
                .Include(i => i.Garage)
                .ToListAsync();

            var results = invoices.Select(i => {
                decimal subTotalParts = i.Parts.Sum(p => p.TotalPrice);
                decimal subTotalLabor = (decimal)i.LaborHours * i.LaborPricePerHour;
                decimal totalBeforeTax = subTotalParts + subTotalLabor;
                bool isVat = i.Garage?.IsVatRegistered ?? false;
                decimal taxAmount = isVat ? (totalBeforeTax * (i.TaxPercentage / 100)) : 0m;
                return new InvoiceFullViewModel
                {
                    Id = i.Id,
                    InvoiceNumber = i.InvoiceNumber,
                    IssuedDate = i.IssuedDate,
                    IsCancelled = i.IsCancelled,
                    PaymentMethod = i.PaymentMethod,
                    PaymentMethodText = GetPaymentMethodDisplayName(i.PaymentMethod),
                    CarInfo = i.Car.Make + " " + i.Car.Model + " (" + i.Car.RegistrationNumber + ")",
                    GrandTotal = totalBeforeTax + taxAmount
                };
            }).ToList();

            if (!string.IsNullOrEmpty(sortBy))
            {
                bool ascending = isAsc ?? true;
                results = sortBy.ToLower() switch
                {
                    "number" => ascending ? results.OrderBy(r => r.InvoiceNumber).ToList() : results.OrderByDescending(r => r.InvoiceNumber).ToList(),
                    "date" => ascending ? results.OrderBy(r => r.IssuedDate).ToList() : results.OrderByDescending(r => r.IssuedDate).ToList(),
                    "car" => ascending ? results.OrderBy(r => r.CarInfo, StringComparer.OrdinalIgnoreCase).ToList() : results.OrderByDescending(r => r.CarInfo, StringComparer.OrdinalIgnoreCase).ToList(),
                    "total" => ascending ? results.OrderBy(r => r.GrandTotal).ToList() : results.OrderByDescending(r => r.GrandTotal).ToList(),
                    _ => results.OrderByDescending(r => r.IssuedDate).ToList()
                };
            }
            else
            {
                results = results.OrderByDescending(r => r.IssuedDate).ToList();
            }

            return results;
        }

        public async Task<InvoiceReportViewModel> GetRevenueReportAsync(string userId, DateTime? startDate, DateTime? endDate)
        {
            var query = context.Invoices
                .Where(i => i.Garage != null && i.Garage.OwnerId == userId && !i.IsCancelled)
                .AsQueryable();

            if (startDate.HasValue)
            {
                query = query.Where(i => i.IssuedDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(i => i.IssuedDate <= endOfDay);
            }

            var invoices = await query
                .Include(i => i.Parts)
                .Include(i => i.Garage)
                .ToListAsync();

            var report = new InvoiceReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                IsGenerated = startDate.HasValue || endDate.HasValue
            };

            foreach (var invoice in invoices)
            {
                decimal subTotalParts = invoice.Parts.Sum(p => p.TotalPrice);
                decimal subTotalLabor = (decimal)invoice.LaborHours * invoice.LaborPricePerHour;
                decimal totalBeforeTax = subTotalParts + subTotalLabor;
                bool isVat = invoice.Garage?.IsVatRegistered ?? false;
                decimal taxAmount = isVat ? (totalBeforeTax * (invoice.TaxPercentage / 100)) : 0m;
                decimal grandTotal = totalBeforeTax + taxAmount;

                switch (invoice.PaymentMethod)
                {
                    case PaymentMethod.Cash:
                        report.CashRevenue += grandTotal;
                        break;
                    case PaymentMethod.Card:
                        report.CardRevenue += grandTotal;
                        break;
                    case PaymentMethod.BankTransfer:
                        report.BankTransferRevenue += grandTotal;
                        break;
                }
            }

            report.TotalRevenue = report.CashRevenue + report.CardRevenue + report.BankTransferRevenue;

            return report;
        }

        public async Task<bool> AnnulInvoiceAsync(int invoiceId, string userId)
        {
            var invoice = await context.Invoices
                .Include(i => i.Garage)
                .FirstOrDefaultAsync(i => i.Id == invoiceId && i.Garage != null && i.Garage.OwnerId == userId);

            if (invoice == null)
            {
                return false;
            }

            invoice.IsCancelled = true;
            await context.SaveChangesAsync();
            return true;
        }

        // Помощен метод за превод на български според енума
        private static string GetPaymentMethodDisplayName(PaymentMethod method)
        {
            return method switch
            {
                PaymentMethod.Cash => "В брой",
                PaymentMethod.Card => "С карта",
                PaymentMethod.BankTransfer => "По банков път",
                _ => "В брой"
            };
        }
    }
}