using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Parts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarGarage.Web.Controllers
{
    [Authorize]
    public class PartsController(IPartsService partsService) : BaseController
    {
        

        public async Task<IActionResult> Index(int carId, string? returnUrl)
        { 
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            
            ViewBag.CarId = carId; ViewBag.ReturnUrl = returnUrl;
            var parts = await partsService.GetPartsByCarIdAsync(carId, userId); return View(parts); }

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
            { model.Categories = await partsService.GetCategoriesAsync();
                return View(model);
            }
            
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            await partsService.AddPartAsync(model, userId);
            return RedirectToAction(nameof(Index), new { carId = model.CarId }); 
        }




        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();



            var model = await partsService.GetPartFormModelByIdAsync(id, userId);
            if (model == null) 
                return NotFound();

            model.Categories = await partsService.GetCategoriesAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PartFormModel model)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();




            if (!ModelState.IsValid)
            {
                model.Categories = await partsService.GetCategoriesAsync();
                return View(model);
            }





            await partsService.UpdatePartAsync(id, model, userId);
            return RedirectToAction(nameof(Index), new { carId = model.CarId });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();




            var part = await partsService.GetPartByIdAsync(id, userId);
            if (part == null) return NotFound();

            var formModel = await partsService.GetPartFormModelByIdAsync(id, userId);
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
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await partsService.DeleteAsync(id, userId  );
            return RedirectToAction(nameof(Index), new { carId = carId });
        }
    }
}