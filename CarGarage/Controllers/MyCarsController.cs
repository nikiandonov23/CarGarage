using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.MyCars;
using CarGarage.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class MyCarsController(IMyCarsService carsService) : BaseController
{
    // Index
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var model = await carsService.GetAllUserCarsAsync(userId);
        return View(model);
    }

    // GET: Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = await Task.FromResult(new CreateCarViewModel());
        return View(model);
    }

    // POST: Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCarViewModel model)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (!ModelState.IsValid) return View(model);

        await carsService.CreateCarAsync(userId, model);

        return RedirectToAction(nameof(Index));
    }

    // GET: API endpoint за VIN
    [HttpGet]
    public async Task<IActionResult> GetCarByVin(string vin)
    {
        var car = await carsService.GetCarInfoByVinAsync(vin);
        if (car == null) return NotFound();

        return Json(car);
    }
}
