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





    
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        var car = await carsService.GetCarByIdAsync(id, userId);

        if (car == null)
        {
            return NotFound();
        }

        return View(car);
    }

   
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = GetUserId();
        var success = await carsService.DeleteCarForUserAsync(id, userId);

        if (!success)
        {
            return BadRequest();
        }

        return RedirectToAction(nameof(Index));
    }

    
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = GetUserId();
        var model = await carsService.GetCarForEditAsync(id, userId);

        if (model == null) return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateCarViewModel model)
    {
        var userId = GetUserId();

        
        model.Id = id;

        
        if (!ModelState.IsValid)
        {
            
            var fresh = await carsService.GetCreateCarViewModelAsync();
            model.MakeList = fresh.MakeList;
            
            model.ModelList = await carsService.GetModelsByMakeAsync(model.MakeId);
            return View(model);
        }

        
        bool success = await carsService.UpdateCarAsync(model, userId);

        if (!success)
        {
            
            return NotFound();
        }

        
        return RedirectToAction(nameof(Index));
    }

    //=================================================================================================
}