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
            
            var userId = GetUserId();

            
            var model = await invoicesService.GetAllUserInvoicesAsync(userId);

            return View(model);
        }








        
        [HttpGet]
        public async Task<IActionResult> Create(int carId, string? returnUrl)
        {
            try
            {
                ViewBag.ReturnUrl = returnUrl; //записвам от де ида беее
                var model = await invoicesService.GetNewInvoiceModelAsync(carId);
                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "MyCars");
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

            var invoiceId = await invoicesService.CreateInvoiceAsync(model);
            return RedirectToAction(nameof(Details), new { id = invoiceId });

        }

        
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
                
                return RedirectToAction(nameof(Index));
            }
        }
    }
}