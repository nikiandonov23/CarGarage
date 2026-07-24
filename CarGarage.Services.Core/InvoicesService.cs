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
                .FirstOrDefaultAsync(c => c.Id == carId && (
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
                .Where(c => c.Id == model.CarId && (
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
                .Include(i => i.Parts)
                .Include(i => i.Garage)
                .FirstOrDefaultAsync(i => i.Id == invoiceId && i.Garage != null && i.Garage.OwnerId == userId);

            if (inv == null) throw new Exception("Invoice not found or access denied");

            decimal subTotalParts = inv.Parts.Sum(p => p.TotalPrice);
            decimal subTotalLabor = (decimal)inv.LaborHours * inv.LaborPricePerHour;
            decimal totalBeforeTax = subTotalParts + subTotalLabor;
            decimal taxAmount = totalBeforeTax * (inv.TaxPercentage / 100);

            return new InvoiceFullViewModel
            {
                Id = invoiceId,
                InvoiceNumber = inv.InvoiceNumber,
                IssuedDate = inv.IssuedDate,

                // --- МАПВАНЕ НА ПЛАЩАНЕТО ---
                PaymentMethod = inv.PaymentMethod,
                PaymentMethodText = GetPaymentMethodDisplayName(inv.PaymentMethod),

                CarInfo = $"{inv.Car.Make} {inv.Car.Model}",
                Vin = inv.Car.Vin,
                RegNumber = inv.Car.RegistrationNumber,
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
                GaragePhoneNumber = inv.Garage?.PhoneNumber
            };
        }

        public async Task<IEnumerable<InvoiceFullViewModel>> GetAllUserInvoicesAsync(string userId)
        {
            var invoices = await context.Invoices
                .Where(i => i.Garage != null && i.Garage.OwnerId == userId)
                .Include(i => i.Car)
                .Include(i => i.Parts)
                .OrderByDescending(i => i.IssuedDate)
                .ToListAsync();

            return invoices.Select(i => new InvoiceFullViewModel
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                IssuedDate = i.IssuedDate,
                PaymentMethod = i.PaymentMethod,
                PaymentMethodText = GetPaymentMethodDisplayName(i.PaymentMethod),
                CarInfo = i.Car.Make + " " + i.Car.Model + " (" + i.Car.RegistrationNumber + ")",
                GrandTotal = i.Parts.Sum(p => p.TotalPrice) + (decimal)i.LaborHours * i.LaborPricePerHour
            });
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