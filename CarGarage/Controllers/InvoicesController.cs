using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Invoices;
using Microsoft.AspNetCore.Mvc;

namespace CarGarage.Web.Controllers
{
    public class InvoicesController(IInvoicesService invoicesService) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await invoicesService.GetAllUserInvoicesAsync(GetUserId());
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int carId, string? returnUrl)
        {
            try
            {
                ViewBag.ReturnUrl = returnUrl;
                var model = await invoicesService.GetNewInvoiceModelAsync(carId, GetUserId());
                return View(model);
            }
            catch (Exception ex) // Добави 'ex' тук
            {
                // Вместо редирект, хвърли грешката на екрана, за да я прочетеш:
                return Content($"Грешка при генериране: {ex.Message}. Вътрешна грешка: {ex.InnerException?.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoiceFormModel model, string? returnUrl)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            try
            {
                var invoiceId = await invoicesService.CreateInvoiceAsync(model, GetUserId());
                return RedirectToAction(nameof(Details), new { id = invoiceId });
            }
            catch (Exception)
            {
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var model = await invoicesService.GetInvoiceDetailsAsync(id, GetUserId());
                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Index));
            }
        }
    }
}