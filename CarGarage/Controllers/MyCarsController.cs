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

        // Първо пишем в конзолата, че сме влезли в метода
        Console.WriteLine("--- POST Create Car Method Started ---");
        Console.WriteLine($"User ID: {userId}");
        Console.WriteLine($"MakeId: {model.MakeId}, ModelId: {model.ModelId}");
        Console.WriteLine($"RegNumber: {model.RegistrationNumber}");

        if (!ModelState.IsValid)
        {
            // Ако има грешки, те ще излязат в CMD промпта тук:
            Console.WriteLine("!!! ModelState is INVALID !!!");

            foreach (var entry in ModelState)
            {
                if (entry.Value.Errors.Count > 0)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        // Това ще се изпише в черния прозорец на CMD
                        Console.WriteLine($"Property: {entry.Key} -> Error: {error.ErrorMessage}");
                    }
                }
            }

            // Презареждаме марките, за да не гръмне дропдауна при връщане на изгледа
            var freshModel = await carsService.GetCreateCarViewModelAsync();
            model.MakeList = freshModel.MakeList;

            return View(model);
        }

        try
        {
            Console.WriteLine("Validation passed. Calling service to save car...");

            await carsService.AddCarToUserAsync(model, userId);

            Console.WriteLine("Car successfully saved to database!");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            // Ако има проблем с базата данни, ще го видиш тук
            Console.WriteLine("!!! DATABASE ERROR !!!");
            Console.WriteLine(ex.Message);

            var freshModel = await carsService.GetCreateCarViewModelAsync();
            model.MakeList = freshModel.MakeList;
            return View(model);
        }
    }
}