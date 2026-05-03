using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Parts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarGarage.Web.Controllers
{
    [Authorize]
    public class PartsController(IPartsService partsService) : Controller
    {
        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        public async Task<IActionResult> Index(int carId, string? returnUrl)
        {
            ViewBag.CarId = carId;
            ViewBag.ReturnUrl = returnUrl;
            var parts = await partsService.GetPartsByCarIdAsync(carId, GetUserId());
            return View(parts);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int carId)
        {
            var model = new PartFormModel
            {
                CarId = carId,
                Categories = await partsService.GetCategoriesAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PartFormModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await partsService.GetCategoriesAsync();
                return View(model);
            }

            await partsService.AddPartAsync(model, GetUserId());
            return RedirectToAction(nameof(Index), new { carId = model.CarId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await partsService.GetPartFormModelByIdAsync(id, GetUserId());
            if (model == null) return NotFound();

            model.Categories = await partsService.GetCategoriesAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PartFormModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await partsService.GetCategoriesAsync();
                return View(model);
            }

            await partsService.UpdatePartAsync(id, model, GetUserId());
            return RedirectToAction(nameof(Index), new { carId = model.CarId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var part = await partsService.GetPartByIdAsync(id, GetUserId());
            if (part == null) return NotFound();

            var formModel = await partsService.GetPartFormModelByIdAsync(id, GetUserId());
            var model = new PartDeleteViewModel
            {
                Id = part.Id,
                CarId = formModel?.CarId ?? 0,
                Description = part.Description,
                CategoryName = part.CategoryName,
                TotalPrice = part.TotalPrice
            };

            return View(model);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int carId)
        {
            await partsService.DeleteAsync(id, GetUserId());
            return RedirectToAction(nameof(Index), new { carId = carId });
        }
    }
}