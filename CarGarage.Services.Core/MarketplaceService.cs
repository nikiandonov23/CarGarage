using CarGarage.Data;
using CarGarage.DataModels;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Marketplace;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Services.Core
{
    public class MarketplaceService(ApplicationDbContext context) : IMarketplaceService
    {
        public async Task AddAsync(PartForSaleFormModel model, string userId)
        {
            var garageId = await context.Garages
                .Where(g => g.OwnerId == userId)
                .Select(g => g.Id)
                .FirstOrDefaultAsync();

            if (garageId == 0) throw new InvalidOperationException("Garage not found for user.");

            var entity = new PartForSale
            {
                Name = model.Name,
                ImageUrl = model.ImageUrl,
                CategoryId = model.CategoryId,
                Price = model.Price,
                Phone = model.Phone,
                Description = model.Description,
                GarageId = garageId,
                OwnerId = userId
            };

            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var entity = await context.Set<PartForSale>()
                .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

            if (entity != null)
            {
                context.Remove(entity);
                await context.SaveChangesAsync();
            }
        }

        public async Task<MarketplaceIndexViewModel> GetMarketplaceAsync(string? searchTerm, string? city, int? categoryId, string? userId, string? ownerId = null)
        {
            var query = context.Set<PartForSale>()
                .Include(p => p.Category)
                .Include(p => p.Garage)
                .AsQueryable();

            // Exclude only sold parts from marketplace -> show Available and Pending
            query = query.Where(p => p.Status != "Sold");

            if (!string.IsNullOrWhiteSpace(ownerId))
            {
                query = query.Where(p => p.OwnerId == ownerId);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(p => p.Garage != null && p.Garage.City != null && p.Garage.City.Contains(city));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var results = await query
                .OrderByDescending(p => p.DateAdded)
                .Select(p => new PartForSaleViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    ImageUrl = p.ImageUrl,
                    CategoryName = p.Category.Name,
                    Price = p.Price,
                    Phone = p.Phone,
                    GarageName = p.Garage != null ? p.Garage.Name : null,
                    City = p.Garage != null ? p.Garage.City : null,
                    OwnerId = p.OwnerId,
                    Status = p.Status,
                    DateAdded = p.DateAdded
                }).ToListAsync();

            var categories = await GetCategoriesAsync();

            return new MarketplaceIndexViewModel
            {
                Results = results,
                SearchTerm = searchTerm,
                City = city,
                CategoryId = categoryId,
                Categories = categories,
                OwnerId = ownerId
            };
        }

        public async Task<PartForSaleViewModel?> GetByIdAsync(int id)
        {
            return await context.Set<PartForSale>()
                .Include(p => p.Category)
                .Include(p => p.Garage)
                .Where(p => p.Id == id)
                .Select(p => new PartForSaleViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    ImageUrl = p.ImageUrl,
                    CategoryName = p.Category.Name,
                    Price = p.Price,
                    Phone = p.Phone,
                    GarageName = p.Garage != null ? p.Garage.Name : null,
                    City = p.Garage != null ? p.Garage.City : null,
                    OwnerId = p.OwnerId,
                    Status = p.Status,
                    DateAdded = p.DateAdded
                }).FirstOrDefaultAsync();
        }

        public async Task<PartForSaleFormModel?> GetForEditAsync(int id, string userId)
        {
            var entity = await context.Set<PartForSale>()
                .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

            if (entity == null) return null;

            var model = new PartForSaleFormModel
            {
                Id = entity.Id,
                Name = entity.Name,
                ImageUrl = entity.ImageUrl,
                CategoryId = entity.CategoryId,
                Price = entity.Price,
                Phone = entity.Phone,
                Description = entity.Description,
                Categories = await GetCategoriesAsync()
            };

            return model;
        }

        public async Task<IEnumerable<CarGarage.ViewModels.Parts.PartCategoryViewModel>> GetCategoriesAsync()
        {
            return await context.PartCategories
                .OrderBy(c => c.Name)
                .Select(c => new CarGarage.ViewModels.Parts.PartCategoryViewModel { Id = c.Id, Name = c.Name })
                .ToListAsync();
        }

        public async Task UpdateAsync(int id, PartForSaleFormModel model, string userId)
        {
            var entity = await context.Set<PartForSale>()
                .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

            if (entity == null) return;

            entity.Name = model.Name;
            entity.ImageUrl = model.ImageUrl;
            entity.CategoryId = model.CategoryId;
            entity.Price = model.Price;
            entity.Phone = model.Phone;
            entity.Description = model.Description;

            await context.SaveChangesAsync();
        }
    }
}
