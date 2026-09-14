using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Marketplace;
using CarGarage.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarGarage.Controllers
{
    public class MarketplaceController(
        IMarketplaceService marketplaceService,
        IGarageService garageService) : BaseController
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

            await marketplaceService.AddAsync(model, userId);
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

            await marketplaceService.UpdateAsync(id, model, userId);
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
    }
}