using CarGarage.DataModels.Enums;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Invoices;
using Microsoft.AspNetCore.Mvc;

namespace CarGarage.Web.Controllers
{
    public class InvoicesController(IInvoicesService invoicesService) : BaseController
    {
        [HttpGet] 
        public async Task<IActionResult> Index(
            string? status = null, 
            PaymentMethod? paymentMethod = null, 
            string? clientSearch = null, 
            string? carSearch = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? sortBy = null,
            bool? isAsc = null)
        {

            var userId = GetUserId(); 

            if (string.IsNullOrEmpty(userId)) 

                return Unauthorized();


            var model = await invoicesService.GetAllUserInvoicesAsync(userId, status, paymentMethod, clientSearch, carSearch, startDate, endDate, sortBy, isAsc);

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentPaymentMethod = paymentMethod;
            ViewBag.CurrentClientSearch = clientSearch;
            ViewBag.CurrentCarSearch = carSearch;
            ViewBag.CurrentStartDate = startDate;
            ViewBag.CurrentEndDate = endDate;
            ViewBag.SortBy = sortBy;
            ViewBag.IsAsc = isAsc;

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

        [HttpGet]
        public async Task<IActionResult> Annul(int id, string? returnUrl)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var model = await invoicesService.GetInvoiceDetailsAsync(id, userId);
            if (model == null)
            {
                return NotFound();
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ActionName("Annul")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnnulConfirmed(int id, string? returnUrl)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await invoicesService.AnnulInvoiceAsync(id, userId);
            if (!result)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        [HttpGet]
        public async Task<IActionResult> Report(DateTime? startDate, DateTime? endDate)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var reportModel = await invoicesService.GetRevenueReportAsync(userId, startDate, endDate);
            return View(reportModel);
        }
    }
}