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
        var model = await carsService.GetAllUserCarsAsync(userId);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var userId = GetUserId();
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
    public async Task<IActionResult> Create(CreateCarViewModel model)
    {
        var userId = GetUserId();

        // РЪЧНА СЪРВЪРНА ВАЛИДАЦИЯ ЗА КЛИЕНТА
        if (!model.IsNewCustomer && !model.CustomerId.HasValue)
        {
            ModelState.AddModelError("CustomerId", "Моля, изберете съществуващ клиент.");
        }
        else if (model.IsNewCustomer)
        {
            // Проверка на общите задължителни полета за базата данни
            if (string.IsNullOrWhiteSpace(model.NewCustomerEmail)) ModelState.AddModelError("NewCustomerEmail", "Имейлът е задължителен.");
            if (string.IsNullOrWhiteSpace(model.NewCustomerPhoneNumber)) ModelState.AddModelError("NewCustomerPhoneNumber", "Телефонният номер е задължителен.");
            if (string.IsNullOrWhiteSpace(model.NewCustomerAddress)) ModelState.AddModelError("NewCustomerAddress", "Адресът е задължителен.");
            if (string.IsNullOrWhiteSpace(model.NewCustomerCity)) ModelState.AddModelError("NewCustomerCity", "Градът е задължителен.");

            if (model.NewCustomerType == "Individual")
            {
                if (string.IsNullOrWhiteSpace(model.NewFirstName)) ModelState.AddModelError("NewFirstName", "Името е задължително.");
                if (string.IsNullOrWhiteSpace(model.NewLastName)) ModelState.AddModelError("NewLastName", "Фамилията е задължителна.");
                if (string.IsNullOrWhiteSpace(model.NewCustomerEgn)) ModelState.AddModelError("NewCustomerEgn", "ЕГН е задължително.");
            }
            else if (model.NewCustomerType == "LegalEntity")
            {
                if (string.IsNullOrWhiteSpace(model.NewCompanyName)) ModelState.AddModelError("NewCompanyName", "Името на фирмата е задължително.");
                if (string.IsNullOrWhiteSpace(model.NewCustomerVatNumber)) ModelState.AddModelError("NewCustomerVatNumber", "ЕИК / БУЛСТАТ е задължителен.");
                if (string.IsNullOrWhiteSpace(model.NewCustomerResponsiblePerson)) ModelState.AddModelError("NewCustomerResponsiblePerson", "МОЛ е задължителен.");
            }
        }

        if (!ModelState.IsValid)
        {
            var freshModel = await carsService.GetCreateCarViewModelAsync(userId);
            model.MakeList = freshModel.MakeList;
            model.CustomerList = freshModel.CustomerList;
            if (model.MakeId > 0)
            {
                model.ModelList = await carsService.GetModelsByMakeAsync(model.MakeId);
            }
            return View(model);
        }

        bool success = await carsService.AddCarToUserAsync(model, userId);

        if (!success)
        {
            ModelState.AddModelError("RegistrationNumber", "Неуспешен запис. Възможно е колата вече да съществува или да липсват задължителни данни.");
            var freshModel = await carsService.GetCreateCarViewModelAsync(userId);
            model.MakeList = freshModel.MakeList;
            model.CustomerList = freshModel.CustomerList;
            if (model.MakeId > 0)
            {
                model.ModelList = await carsService.GetModelsByMakeAsync(model.MakeId);
            }
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
            var fresh = await carsService.GetCreateCarViewModelAsync(userId);
            model.MakeList = fresh.MakeList;
            model.CustomerList = fresh.CustomerList;
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