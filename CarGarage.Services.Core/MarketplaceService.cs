using CarGarage.Data;
using CarGarage.DataModels;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Marketplace;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Services.Core
{
    public class MarketplaceService(ApplicationDbContext context, ICloudflareR2Service r2Service) : IMarketplaceService
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

            // Handle file uploads on create (up to 5)
            if (model.ImageFiles != null && model.ImageFiles.Any())
            {
                var filesToUpload = model.ImageFiles.Take(5);
                int currentOrder = 0;
                foreach (var file in filesToUpload)
                {
                    if (file == null || file.Length == 0) continue;
                    try
                    {
                        var (uploadedUrl, key) = await r2Service.UploadImageAsync(file, "marketplace-images");
                        entity.PartForSaleImages.Add(new PartForSaleImage
                        {
                            ImageUrl = uploadedUrl,
                            StorageKey = key,
                            SortOrder = currentOrder++
                        });
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Грешка при запис във R2 (Marketplace Добавяне): {ex.Message}. Вътрешна грешка: {ex.InnerException?.Message}", ex);
                    }
                }
            }

            await context.AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var entity = await context.Set<PartForSale>()
                .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

            if (entity != null)
            {
                var images = await context.PartForSaleImages.Where(img => img.PartForSaleId == id).ToListAsync();
                foreach (var img in images)
                {
                    await r2Service.DeleteImageAsync(img.StorageKey, "marketplace-images");
                }
                context.PartForSaleImages.RemoveRange(images);

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
                    ImageUrl = p.PartForSaleImages.OrderBy(img => img.SortOrder).ThenBy(img => img.Id).Select(img => img.ImageUrl).FirstOrDefault() ?? p.ImageUrl,
                    PartImages = p.PartForSaleImages.OrderBy(img => img.SortOrder).ThenBy(img => img.Id).Select(img => new PartForSaleImageViewModel
                    {
                        Id = img.Id,
                        ImageUrl = img.ImageUrl,
                        SortOrder = img.SortOrder
                    }).ToList(),
                    CategoryName = p.Category.Name,
                    Price = p.Price,
                    Phone = p.Phone,
                    GarageName = p.Garage != null ? p.Garage.Name : null,
                    City = p.Garage != null ? p.Garage.City : null,
                    OwnerId = p.OwnerId,
                    Status = p.Status,
                    DateAdded = p.DateAdded,
                    Description = p.Description ?? string.Empty
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
                    ImageUrl = p.PartForSaleImages.OrderBy(img => img.SortOrder).ThenBy(img => img.Id).Select(img => img.ImageUrl).FirstOrDefault() ?? p.ImageUrl,
                    PartImages = p.PartForSaleImages.OrderBy(img => img.SortOrder).ThenBy(img => img.Id).Select(img => new PartForSaleImageViewModel
                    {
                        Id = img.Id,
                        ImageUrl = img.ImageUrl,
                        SortOrder = img.SortOrder
                    }).ToList(),
                    CategoryName = p.Category.Name,
                    Price = p.Price,
                    Phone = p.Phone,
                    GarageName = p.Garage != null ? p.Garage.Name : null,
                    City = p.Garage != null ? p.Garage.City : null,
                    OwnerId = p.OwnerId,
                    Status = p.Status,
                    DateAdded = p.DateAdded,
                    Description = p.Description ?? string.Empty
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

            model.ExistingImages = await context.PartForSaleImages
                .Where(img => img.PartForSaleId == entity.Id)
                .OrderBy(img => img.SortOrder)
                .ThenBy(img => img.Id)
                .Select(img => new PartForSaleImageViewModel
                {
                    Id = img.Id,
                    ImageUrl = img.ImageUrl,
                    SortOrder = img.SortOrder
                })
                .ToListAsync();

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
            if (!string.IsNullOrEmpty(model.ImageUrl))
            {
                entity.ImageUrl = model.ImageUrl;
            }
            entity.CategoryId = model.CategoryId;
            entity.Price = model.Price;
            entity.Phone = model.Phone;
            entity.Description = model.Description;

            // Handle new image uploads on edit (maximum 5 total images)
            if (model.ImageFiles != null && model.ImageFiles.Any())
            {
                var currentCount = await context.PartForSaleImages.CountAsync(img => img.PartForSaleId == entity.Id);
                var maxOrder = await context.PartForSaleImages.Where(img => img.PartForSaleId == entity.Id).Select(img => (int?)img.SortOrder).MaxAsync() ?? -1;
                var allowedSlots = 5 - currentCount;
                if (allowedSlots > 0)
                {
                    var filesToUpload = model.ImageFiles.Take(allowedSlots);
                    int currentOrder = maxOrder + 1;
                    foreach (var file in filesToUpload)
                    {
                        if (file == null || file.Length == 0) continue;
                        try
                        {
                            var (uploadedUrl, key) = await r2Service.UploadImageAsync(file, "marketplace-images");
                            var newImg = new PartForSaleImage
                            {
                                PartForSaleId = entity.Id,
                                ImageUrl = uploadedUrl,
                                StorageKey = key,
                                SortOrder = currentOrder++
                            };
                            await context.PartForSaleImages.AddAsync(newImg);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Грешка при запис във R2 (Marketplace Редактиране): {ex.Message}. Вътрешна грешка: {ex.InnerException?.Message}", ex);
                        }
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        public async Task<bool> DeletePartImageAsync(int partId, int imageId, string userId)
        {
            var partExists = await context.Set<PartForSale>()
                .AnyAsync(p => p.Id == partId && p.OwnerId == userId);

            if (!partExists) return false;

            var image = await context.PartForSaleImages
                .FirstOrDefaultAsync(img => img.Id == imageId && img.PartForSaleId == partId);

            if (image == null) return false;

            // Delete from Cloudflare R2
            await r2Service.DeleteImageAsync(image.StorageKey, "marketplace-images");

            // Delete from DB
            context.PartForSaleImages.Remove(image);
            await context.SaveChangesAsync();

            return true;
        }
    }
}
