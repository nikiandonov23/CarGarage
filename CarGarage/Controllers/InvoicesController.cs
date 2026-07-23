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

            if (string.IsNullOrEmpty(userId)) 

                return Unauthorized();


            var model = await invoicesService.GetAllUserInvoicesAsync(userId);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int carId, string? returnUrl)
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            ViewBag.ReturnUrl = returnUrl;
            var model = await invoicesService.GetNewInvoiceModelAsync(carId, userId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Create(InvoiceFormModel model, string? returnUrl) 
        { 
            if (!ModelState.IsValid) 
            { ViewBag.ReturnUrl = returnUrl;
                return View(model); 
            }
            
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            
            var invoiceId = await invoicesService.CreateInvoiceAsync(model, userId);
            
            return RedirectToAction(nameof(Details), new { id = invoiceId }); 
        
        }

        [HttpGet] public async Task<IActionResult> Details(int id) 
        {
            var userId = GetUserId();
            
            if (string.IsNullOrEmpty(userId)) 
                
                return Unauthorized();
            
            
            var model = await invoicesService.GetInvoiceDetailsAsync(id, userId);
            
            
            if (model == null) 
                return RedirectToAction(nameof(Index)); return View(model);
        }
    }
}