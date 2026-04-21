using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Invoices;
using Microsoft.AspNetCore.Mvc;

namespace CarGarage.Web.Controllers
{
    public class InvoicesController(IInvoicesService invoicesService) : BaseController
    {


        // 0. Списък с всички фактури (Архив)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Взимаме ID-то на логнатия потребител чрез метода от BaseController
            var userId = GetUserId();

            // Викаме новия метод от сървиса
            var model = await invoicesService.GetAllUserInvoicesAsync(userId);

            return View(model);
        }








        // 1. Форма за създаване на фактура
        [HttpGet]
        public async Task<IActionResult> Create(int carId)
        {
            try
            {
                var model = await invoicesService.GetNewInvoiceModelAsync(carId);
                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "MyCars");
            }
        }

        // 2. Обработка на формата
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoiceFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var invoiceId = await invoicesService.CreateInvoiceAsync(model);

            // Пренасочваме към изгледа за принтиране/преглед
            return RedirectToAction(nameof(Details), new { id = invoiceId });
        }

        // 3. Преглед на готовата фактура (Details)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var model = await invoicesService.GetInvoiceDetailsAsync(id);
                return View(model);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
    }
}