using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.MyCars;
using CarGarage.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class MyCarsController : BaseController
{
    private readonly IMyCarsService _carsService;

    public MyCarsController(IMyCarsService carsService)
    {
        _carsService = carsService;
    }





    // Index - списък с всички автомобили на потребителя
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var model = await _carsService.GetAllUserCarsAsync(userId);
        return View(model);
    }







    // GET: Create - показва празната форма
    [HttpGet]
    public IActionResult Create()   // премахнах async + Task.FromResult
    {
        return View(new CreateCarViewModel());
    }







    // POST: Create - запазва автомобила
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCarViewModel model)
    {
        Console.WriteLine("══════════════════════════════════════");
        Console.WriteLine("CREATE POST - ЗАПОЧВА");

        Console.WriteLine($"Make = '{model.Make}'");
        Console.WriteLine($"Model = '{model.Model}'");
        Console.WriteLine($"RegistrationNumber = '{model.RegistrationNumber}'");
        Console.WriteLine($"ModelYear = {model.ModelYear}");
        Console.WriteLine($"Vin = '{model.Vin}'");
        Console.WriteLine($"ModelState.IsValid = {ModelState.IsValid}");

        if (!ModelState.IsValid)
        {
            Console.WriteLine("--- ModelState ГРЕШКИ ---");
            foreach (var entry in ModelState)
            {
                if (entry.Value.Errors.Count > 0)
                {
                    var errors = string.Join(" | ", entry.Value.Errors.Select(e => e.ErrorMessage));
                    Console.WriteLine($"  {entry.Key}: {errors}");
                }
            }

            // По-добър check за AntiForgery проблем
            if (ModelState.ContainsKey("__RequestVerificationToken"))
            {
                Console.WriteLine("ВНИМАНИЕ: Проблем с AntiForgeryToken!");
            }
            else if (ModelState.ErrorCount > 0)
            {
                Console.WriteLine($"Общо грешки в ModelState: {ModelState.ErrorCount}");
            }

            return View(model);
        }

        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
        {
            Console.WriteLine("ГРЕШКА: UserId е null!");
            return Unauthorized();
        }

        try
        {
            await _carsService.CreateCarAsync(userId, model);
            Console.WriteLine("УСПЕШНО създаден автомобил!");
            TempData["SuccessMessage"] = "Автомобилът беше добавен успешно!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ГРЕШКА: {ex.Message}");
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }








    // GET: API endpoint за VIN decode (използва се от JavaScript)
    [HttpGet]
    public async Task<IActionResult> GetCarByVin(string vin)
    {
        if (string.IsNullOrWhiteSpace(vin))
            return BadRequest("VIN номерът е задължителен");

        var carInfo = await _carsService.GetCarInfoByVinAsync(vin);

        if (carInfo == null)
            return NotFound(new { message = "Не успяхме да декодираме VIN номера." });

        return Json(carInfo);
    }







}