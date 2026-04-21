using CarGarage.Data;
using CarGarage.DataModels;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Invoices;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Services.Core
{
    public class InvoicesService(ApplicationDbContext context) : IInvoicesService
    {





        public async Task<InvoiceFormModel> GetNewInvoiceModelAsync(int carId)
        {
            var car = await context.Cars
                .FirstOrDefaultAsync(c => c.Id == carId);

            if (car == null) throw new Exception("Car not found");

            var parts = await context.Parts
                .Where(p => p.CarId == carId && p.InvoiceId == null)
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

        public async Task<int> CreateInvoiceAsync(InvoiceFormModel model)
        {
            var invoice = new Invoice
            {
                CarId = model.CarId,
                IssuedDate = DateTime.UtcNow,
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{new Random().Next(1000, 9999)}",
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
                .Where(p => selectedIds.Contains(p.Id))
                .ToListAsync();

            foreach (var part in parts)
            {
                invoice.Parts.Add(part);
            }

            await context.Invoices.AddAsync(invoice);
            await context.SaveChangesAsync();

            return invoice.Id;
        }

        public async Task<InvoiceFullViewModel> GetInvoiceDetailsAsync(int invoiceId)
        {
            var inv = await context.Invoices
                .Include(i => i.Car)
                .Include(i => i.Parts)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (inv == null) throw new Exception("Invoice not found");

            decimal subTotalParts = inv.Parts.Sum(p => p.TotalPrice);
            decimal subTotalLabor = (decimal)inv.LaborHours * inv.LaborPricePerHour;
            decimal totalBeforeTax = subTotalParts + subTotalLabor;
            decimal taxAmount = totalBeforeTax * (inv.TaxPercentage / 100);

            return new InvoiceFullViewModel
            {
                Id = invoiceId,
                InvoiceNumber = inv.InvoiceNumber,
                IssuedDate = inv.IssuedDate,
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
                Notes = inv.Notes
            };
        }




        public async Task<IEnumerable<InvoiceFullViewModel>> GetAllUserInvoicesAsync(string userId)
        {
            return await context.Invoices
                .Where(i => i.Car.UserCars.Any(uc => uc.UserId == userId)) 
                .Select(i => new InvoiceFullViewModel
                {
                    Id = i.Id,
                    InvoiceNumber = i.InvoiceNumber,
                    IssuedDate = i.IssuedDate,
                    CarInfo = i.Car.Make + " " + i.Car.Model + " (" + i.Car.RegistrationNumber + ")",
                    GrandTotal = i.Parts.Sum(p => p.UnitPrice * p.Quantity) + (decimal)i.LaborHours * i.LaborPricePerHour
                   
                })
                .OrderByDescending(i => i.IssuedDate)
                .ToListAsync();
        }
    }
}