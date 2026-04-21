using CarGarage.ViewModels.Cars;
using CarGarage.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class MyCarsController(IMyCarsService carsService) : BaseController
{
    // 1. Списък с коли
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var model = await carsService.GetAllUserCarsAsync(userId);
        return View(model);
    }

    // 2. GET: Показва формата с напълнени марки
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        // Вече викаме сървиса, а не просто нов празен обект
        var model = await carsService.GetCreateCarViewModelAsync();
        return View(model);
    }

    // 3. AJAX: Връща моделите
    [HttpGet]
    public async Task<IActionResult> GetModelsByMake(int makeId)
    {
        var models = await carsService.GetModelsByMakeAsync(makeId);
        return Json(models);
    }

    // 4. POST: Записва данните
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCarViewModel model)
    {
        var userId = GetUserId();

        if (!ModelState.IsValid)
        {
            var freshModel = await carsService.GetCreateCarViewModelAsync();
            model.MakeList = freshModel.MakeList;
            return View(model);
        }

        // ТУК вече няма да свети в червено:
        bool success = await carsService.AddCarToUserAsync(model, userId);

        if (!success)
        {
            // Показваме грешката в горната част на формата или до полето
            ModelState.AddModelError("RegistrationNumber", "Вече сте добавили кола с този регистрационен номер или VIN.");

            var freshModel = await carsService.GetCreateCarViewModelAsync();
            model.MakeList = freshModel.MakeList;
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }
}