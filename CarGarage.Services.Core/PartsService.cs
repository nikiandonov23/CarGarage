using CarGarage.Data;
using CarGarage.DataModels;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Parts;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Services.Core
{
    public class PartsService(ApplicationDbContext context) : IPartsService
    {
        public async Task<IEnumerable<PartViewModel>> GetPartsByCarIdAsync(int carId, string userId)
        {
            return await context.Parts
                .Where(p => p.CarId == carId && p.Garage.OwnerId == userId) // Филтър по потребител
                .OrderByDescending(p => p.Id)
                .Select(p => new PartViewModel
                {
                    Id = p.Id,
                    CategoryName = p.Category.Name,
                    Description = p.Description,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice,
                    TotalPrice = p.Quantity * p.UnitPrice,
                    DateAdded = DateTime.Now,
                    InvoiceId = p.InvoiceId,
                }).ToListAsync();
        }

        public async Task<PartViewModel?> GetPartByIdAsync(int id, string userId)
        {
            return await context.Parts
                .Where(p => p.Id == id && p.Garage.OwnerId == userId)
                .Select(p => new PartViewModel
                {
                    Id = p.Id,
                    CategoryName = p.Category.Name,
                    Description = p.Description,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice,
                    TotalPrice = p.Quantity * p.UnitPrice,
                    CarInfo = p.Car.Make + " " + p.Car.Model + " [" + p.Car.RegistrationNumber + "]",
                    DateAdded = DateTime.Now,
                    InvoiceId = p.InvoiceId,
                }).FirstOrDefaultAsync();
        }

        public async Task<PartFormModel?> GetPartFormModelByIdAsync(int id, string userId)
        {
            return await context.Parts
                .Where(p => p.Id == id && p.Garage.OwnerId == userId)
                .Select(p => new PartFormModel
                {
                    CarId = p.CarId,
                    CategoryId = p.CategoryId,
                    Description = p.Description,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice,
                    GarageId = p.GarageId
                }).FirstOrDefaultAsync();
        }

        public async Task AddPartAsync(PartFormModel model, string userId)
        {
            // Намираме GarageId на логнатия потребител
            var garageId = await context.Garages
                .Where(g => g.OwnerId == userId)
                .Select(g => g.Id)
                .FirstOrDefaultAsync();

            if (garageId == 0) throw new InvalidOperationException("Гаражът не е намерен.");

            var part = new Part
            {
                CarId = model.CarId,
                CategoryId = model.CategoryId,
                Description = model.Description,
                Quantity = model.Quantity,
                UnitPrice = model.UnitPrice,
                GarageId = garageId // Автоматично присвояване
            };

            await context.Parts.AddAsync(part);
            await context.SaveChangesAsync();
        }

        public async Task UpdatePartAsync(int id, PartFormModel model, string userId)
        {
            var part = await context.Parts
                .FirstOrDefaultAsync(p => p.Id == id && p.Garage.OwnerId == userId);

            if (part != null)
            {
                part.CategoryId = model.CategoryId;
                part.Description = model.Description;
                part.Quantity = model.Quantity;
                part.UnitPrice = model.UnitPrice;

                await context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var part = await context.Parts
                .FirstOrDefaultAsync(p => p.Id == id && p.Garage.OwnerId == userId);

            if (part != null)
            {
                context.Parts.Remove(part);
                await context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<PartCategoryViewModel>> GetCategoriesAsync()
        {
            return await context.PartCategories
                .OrderBy(c => c.Name)
                .Select(c => new PartCategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToListAsync();
        }
    }
}