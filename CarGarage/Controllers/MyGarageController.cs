using CarGarage.ViewModels.Garage;
using CarGarage.Web.Controllers;
using Microsoft.AspNetCore.Mvc;

public class MyGarageController : BaseController
{
    private readonly IGarageService _garageService;

    public MyGarageController(IGarageService garageService)
    {
        _garageService = garageService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetUserId(); // Вземи ID-то от BaseController
        var hasGarage = await _garageService.HasGarageAsync(userId);

        ViewBag.HasGarage = hasGarage;
        return View();
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(GarageViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        await _garageService.CreateGarageAsync(model, GetUserId());
        return RedirectToAction(nameof(Index));
    }


    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var model = await _garageService.GetGarageDetailsAsync(GetUserId());
        if (model == null) return RedirectToAction(nameof(Register));

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(GarageViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        await _garageService.UpdateGarageAsync(model, GetUserId());
        return RedirectToAction(nameof(Index));
    }
}