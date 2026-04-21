using CarGarage.Data;
using CarGarage.DataModels;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Parts;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Services.Core
{
    public class PartsService : IPartsService
    {
        private readonly ApplicationDbContext context;

        public PartsService(ApplicationDbContext _context)
        {
            context = _context;
        }

        public async Task<IEnumerable<PartViewModel>> GetPartsByCarIdAsync(int carId)
        {
            return await context.Parts
                .Where(p => p.CarId == carId)
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
                    InvoiceId = p.InvoiceId
                })
                .ToListAsync();
        }

        public async Task<PartViewModel?> GetPartByIdAsync(int id)
        {
            return await context.Parts
                .Where(p => p.Id == id)
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
                    InvoiceId = p.InvoiceId
                })
                .FirstOrDefaultAsync();
        }

       
        public async Task<PartFormModel?> GetPartFormModelByIdAsync(int id)
        {
            return await context.Parts
                .Where(p => p.Id == id)
                .Select(p => new PartFormModel
                {
                    CarId = p.CarId,
                    CategoryId = p.CategoryId,
                    Description = p.Description,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice
                })
                .FirstOrDefaultAsync();
        }

        public async Task AddPartAsync(PartFormModel model)
        {
            var part = new Part
            {
                CarId = model.CarId,
                CategoryId = model.CategoryId,
                Description = model.Description,
                Quantity = model.Quantity,
                UnitPrice = model.UnitPrice
            };

            await context.Parts.AddAsync(part);
            await context.SaveChangesAsync();
        }

        
        public async Task UpdatePartAsync(int id, PartFormModel model)
        {
            var part = await context.Parts.FindAsync(id);
            if (part != null)
            {
                part.CategoryId = model.CategoryId;
                part.Description = model.Description;
                part.Quantity = model.Quantity;
                part.UnitPrice = model.UnitPrice;

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
                })
                .ToListAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var part = await context.Parts.FindAsync(id);
            if (part != null)
            {
                context.Parts.Remove(part);
                await context.SaveChangesAsync();
            }
        }


      
    }
}