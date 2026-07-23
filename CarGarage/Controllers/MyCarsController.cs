using CarGarage.ViewModels.Cars;
using CarGarage.ViewModels.Cars.Dropdowns;
using CarGarage.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class MyCarsController(IMyCarsService carsService) : BaseController
{
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();


        var model = await carsService.GetAllUserCarsAsync(userId);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var userId = GetUserId(); 
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();


        var model = await carsService.GetCreateCarViewModelAsync(userId);
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
    public async Task<IActionResult> Create(CreateCarViewModel carModel)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // РЪЧНА СЪРВЪРНА ВАЛИДАЦИЯ ЗА КЛИЕНТА
        if (!carModel.IsNewCustomer && !carModel.CustomerId.HasValue)
        {
            ModelState.AddModelError("CustomerId", "Моля, изберете съществуващ клиент.");
        }
        else if (carModel.IsNewCustomer)
        {
            // Проверка на общите задължителни полета за базата данни
            if (string.IsNullOrWhiteSpace(carModel.NewCustomerEmail)) ModelState.AddModelError("NewCustomerEmail", "Имейлът е задължителен.");
            if (string.IsNullOrWhiteSpace(carModel.NewCustomerPhoneNumber)) ModelState.AddModelError("NewCustomerPhoneNumber", "Телефонният номер е задължителен.");
            if (string.IsNullOrWhiteSpace(carModel.NewCustomerAddress)) ModelState.AddModelError("NewCustomerAddress", "Адресът е задължителен.");
            if (string.IsNullOrWhiteSpace(carModel.NewCustomerCity)) ModelState.AddModelError("NewCustomerCity", "Градът е задължителен.");

            if (carModel.NewCustomerType == "Individual")
            {
                if (string.IsNullOrWhiteSpace(carModel.NewFirstName)) ModelState.AddModelError("NewFirstName", "Името е задължително.");
                if (string.IsNullOrWhiteSpace(carModel.NewLastName)) ModelState.AddModelError("NewLastName", "Фамилията е задължителна.");
                if (string.IsNullOrWhiteSpace(carModel.NewCustomerEgn)) ModelState.AddModelError("NewCustomerEgn", "ЕГН е задължително.");
            }
            else if (carModel.NewCustomerType == "LegalEntity")
            {
                if (string.IsNullOrWhiteSpace(carModel.NewCompanyName)) ModelState.AddModelError("NewCompanyName", "Името на фирмата е задължително.");
                if (string.IsNullOrWhiteSpace(carModel.NewCustomerVatNumber)) ModelState.AddModelError("NewCustomerVatNumber", "ЕИК / БУЛСТАТ е задължителен.");
                if (string.IsNullOrWhiteSpace(carModel.NewCustomerResponsiblePerson)) ModelState.AddModelError("NewCustomerResponsiblePerson", "МОЛ е задължителен.");
            }
        }

        if (!ModelState.IsValid)
        {
            var freshModel = await carsService.GetCreateCarViewModelAsync(userId);
            carModel.MakeList = freshModel.MakeList;
            carModel.CustomerList = freshModel.CustomerList;
            if (carModel.MakeId > 0)
            {
                carModel.ModelList = await carsService.GetModelsByMakeAsync(carModel.MakeId);
            }
            return View(carModel);
        }

        bool success = await carsService.AddCarToUserAsync(carModel, userId);

        if (!success)
        {
            ModelState.AddModelError("RegistrationNumber", "Неуспешен запис. Възможно е колата вече да съществува или да липсват задължителни данни.");
            var freshModel = await carsService.GetCreateCarViewModelAsync(userId);
            carModel.MakeList = freshModel.MakeList;
            carModel.CustomerList = freshModel.CustomerList;
            if (carModel.MakeId > 0)
            {
                carModel.ModelList = await carsService.GetModelsByMakeAsync(carModel.MakeId);
            }
            return View(carModel);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, string returnUrl = null)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();


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
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();



        var success = await carsService.DeleteCarForUserAsync(id, userId);

        if (!success) return BadRequest();

        if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, string returnUrl = null)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();



        var model = await carsService.GetCarForEditAsync(id, userId);

        if (model == null) return NotFound();

        ViewData["ReturnUrl"] = returnUrl;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateCarViewModel carModel, string returnUrl = null)
    {
        var userId = GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();



        carModel.Id = id;

        if (!ModelState.IsValid)
        {
            var fresh = await carsService.GetCreateCarViewModelAsync(userId);
            carModel.MakeList = fresh.MakeList;
            carModel.CustomerList = fresh.CustomerList;
            carModel.ModelList = await carsService.GetModelsByMakeAsync(carModel.MakeId);
            ViewData["ReturnUrl"] = returnUrl;
            return View(carModel);
        }

        bool success = await carsService.UpdateCarAsync(carModel, userId);

        if (!success) return NotFound();

        if (!string.IsNullOrEmpty(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction(nameof(Index));
    }
}