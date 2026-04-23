using CarGarage.Services.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CarGarage.Web.Controllers
{
    // Използваме Primary Constructor за двата сървиса
    public class CustomersController(
        ICustomersService customersService,
        IInvoicesService invoicesService) : Controller
    {
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await customersService.GetAllCustomersAsync(searchTerm);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var model = await customersService.GetCustomerDetailsAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        
        public async Task<IActionResult> PrintInvoice(int id)
        {
            
            var model = await invoicesService.GetInvoiceDetailsAsync(id);

            if (model == null) return NotFound();

            
            return View("~/Views/Invoices/Details.cshtml", model);
        }
    }
}