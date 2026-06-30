using CarGarage.Data;
using CarGarage.DataModels;
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
                .FirstOrDefaultAsync(c => c.Id == carId && c.Customer.Garage.OwnerId == userId);

            if (car == null) throw new Exception("Car not found or access denied");

            var parts = await context.Parts
                .Where(p => p.CarId == carId &&
                            p.InvoiceId == null &&
                            p.Car.Customer.Garage.OwnerId == userId)
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
                .Where(c => c.Id == model.CarId && c.Customer.Garage.OwnerId == userId)
                .Select(c => new { c.Customer.GarageId })
                .FirstOrDefaultAsync();

            if (carData == null)
            {
                throw new InvalidOperationException("Garage not found or access denied for this car.");
            }

            var invoice = new Invoice
            {
                CarId = model.CarId,
                GarageId = carData.GarageId,
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
                .Where(p => selectedIds.Contains(p.Id) && p.Car.Customer.Garage.OwnerId == userId)
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
                .FirstOrDefaultAsync(i => i.Id == invoiceId && i.Garage.OwnerId == userId);

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
                .Where(i => i.Garage.OwnerId == userId)
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