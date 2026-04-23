using CarGarage.ViewModels.Cars;
using CarGarage.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class MyCarsController(IMyCarsService carsService) : BaseController
{
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        var model = await carsService.GetAllUserCarsAsync(userId);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = await carsService.GetCreateCarViewModelAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetModelsByMake(int makeId)
    {
        var models = await carsService.GetModelsByMakeAsync(makeId);
        return Json(models);
    }

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

        bool success = await carsService.AddCarToUserAsync(model, userId);

        if (!success)
        {
            ModelState.AddModelError("RegistrationNumber", "Вече сте добавили кола с този регистрационен номер или VIN.");
            var freshModel = await carsService.GetCreateCarViewModelAsync();
            model.MakeList = freshModel.MakeList;
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, string returnUrl = null)
    {
        var userId = GetUserId();
        var car = await carsService.GetCarByIdAsync(id, userId);

        if (car == null) return NotFound();

        ViewData["ReturnUrl"] = returnUrl;
        return View(car);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, string returnUrl = null)
    {
        var userId = GetUserId();
        var success = await carsService.DeleteCarForUserAsync(id, userId);

        if (!success) return BadRequest();

        if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, string returnUrl = null)
    {
        var userId = GetUserId();
        var model = await carsService.GetCarForEditAsync(id, userId);

        if (model == null) return NotFound();

        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateCarViewModel model, string returnUrl = null)
    {
        var userId = GetUserId();
        model.Id = id;

        if (!ModelState.IsValid)
        {
            var fresh = await carsService.GetCreateCarViewModelAsync();
            model.MakeList = fresh.MakeList;
            model.ModelList = await carsService.GetModelsByMakeAsync(model.MakeId);
            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        bool success = await carsService.UpdateCarAsync(model, userId);

        if (!success) return NotFound();

        if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction(nameof(Index));
    }
}