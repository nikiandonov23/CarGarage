using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Marketplace;
using CarGarage.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarGarage.Controllers
{
    public class MarketplaceController(
        IMarketplaceService marketplaceService,
        IGarageService garageService,
        ICloudflareR2Service r2Service) : BaseController
    {
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? searchTerm, string? city, int? categoryId, string? ownerId)
        {
            var userId = GetUserId();
            var model = await marketplaceService.GetMarketplaceAsync(searchTerm, city, categoryId, userId, ownerId);
            return View(model);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var item = await marketplaceService.GetByIdAsync(id);
            if (item == null) return NotFound();

            var userId = GetUserId();
            ViewBag.IsOwner = !string.IsNullOrEmpty(userId) && item.OwnerId == userId;

            return View(item);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var userId = GetUserId();
            var garage = await garageService.GetGarageDetailsAsync(userId!);

            var model = new PartForSaleFormModel
            {
                Categories = await marketplaceService.GetCategoriesAsync(),
                Phone = garage?.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PartForSaleFormModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await marketplaceService.GetCategoriesAsync();
                return View(model);
            }

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            try
            {
                await marketplaceService.AddAsync(model, userId);
            }
            catch (NotSupportedException ex)
            {
                ModelState.AddModelError("ImageFiles", ex.Message);
                model.Categories = await marketplaceService.GetCategoriesAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Възникна неочаквана грешка при запазване: {ex.Message}");
                model.Categories = await marketplaceService.GetCategoriesAsync();
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var model = await marketplaceService.GetForEditAsync(id, userId);
            if (model == null) return NotFound();

            model.Categories = await marketplaceService.GetCategoriesAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PartForSaleFormModel model)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                model.Categories = await marketplaceService.GetCategoriesAsync();
                return View(model);
            }

            try
            {
                await marketplaceService.UpdateAsync(id, model, userId);
            }
            catch (NotSupportedException ex)
            {
                ModelState.AddModelError("ImageFiles", ex.Message);
                model.Categories = await marketplaceService.GetCategoriesAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Възникна неочаквана грешка при запазване: {ex.Message}");
                model.Categories = await marketplaceService.GetCategoriesAsync();
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var item = await marketplaceService.GetByIdAsync(id);
            if (item == null) return NotFound();

            var model = new PartForSaleDeleteViewModel
            {
                Id = item.Id,
                Name = item.Name,
                CategoryName = item.CategoryName,
                Price = item.Price
            };

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await marketplaceService.DeleteAsync(id, userId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int partId, int imageId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var success = await marketplaceService.DeletePartImageAsync(partId, imageId, userId);
            if (success)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Неуспешно изтриване на снимката от хранилището." });
        }

        [HttpGet("/Marketplace/Image/{key}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetImage(string key)
        {
            if (string.IsNullOrEmpty(key)) return NotFound();

            var stream = await r2Service.GetImageStreamAsync(key, "marketplace-images");
            if (stream == null) return NotFound();

            return File(stream, "image/jpeg");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateImagePositions([FromBody] ImagePositionUpdateModel model)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            if (model == null || model.ImageIds == null) return BadRequest();

            var listing = await marketplaceService.GetByIdAsync(model.ItemId);
            if (listing == null || listing.OwnerId != userId) return Forbid();

            // Resolve context from DI container or service (Since Controller parameters have context, we can query it)
            // But wait, the context is not directly in the controller constructor. 
            // We can inject ApplicationDbContext to the constructor or use HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>()
            var context = (CarGarage.Data.ApplicationDbContext)HttpContext.RequestServices.GetService(typeof(CarGarage.Data.ApplicationDbContext))!;

            for (int i = 0; i < model.ImageIds.Count; i++)
            {
                var imgId = model.ImageIds[i];
                var img = await context.PartForSaleImages.FirstOrDefaultAsync(pfi => pfi.Id == imgId && pfi.PartForSaleId == model.ItemId);
                if (img != null)
                {
                    img.SortOrder = i;
                }
            }

            await context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public class ImagePositionUpdateModel
        {
            public int ItemId { get; set; }
            public List<int> ImageIds { get; set; } = new();
        }
    }
}