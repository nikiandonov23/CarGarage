using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Parts;
using Microsoft.AspNetCore.Mvc;

namespace CarGarage.Web.Controllers
{
    public class PartsController(IPartsService partsService) : Controller
    {

        public async Task<IActionResult> Index(int carId, string? returnUrl)
        {
            ViewBag.CarId = carId;
            ViewBag.ReturnUrl = returnUrl; // записвам от де идаааа !!!
            var parts = await partsService.GetPartsByCarIdAsync(carId);
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

            await partsService.AddPartAsync(model);
            return RedirectToAction(nameof(Index), new { carId = model.CarId });
        }

        
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await partsService.GetPartFormModelByIdAsync(id);

            if (model == null)
            {
                return NotFound();
            }

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

            await partsService.UpdatePartAsync(id, model);
            return RedirectToAction(nameof(Index), new { carId = model.CarId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var part = await partsService.GetPartByIdAsync(id);

            if (part == null)
            {
                return NotFound();
            }

            
            var formModel = await partsService.GetPartFormModelByIdAsync(id);

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
            await partsService.DeleteAsync(id);
            return RedirectToAction(nameof(Index), new { carId = carId });
        }

        
        public async Task<IActionResult> PrintInvoice(int id)
        {
            var part = await partsService.GetPartByIdAsync(id);
            if (part == null) return NotFound();

            return View(part);
        }
    }
}