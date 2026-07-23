using System.Security.Claims;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarGarage.Web.Controllers
{
    [Authorize]
    public class CustomersController(
        ICustomersService customersService,
        IInvoicesService invoicesService) : BaseController
    {




        public async Task<IActionResult> Index(string? searchTerm)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();


            var model = await customersService.GetAllCustomersAsync(searchTerm, userId);
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();



            var model = await customersService.GetCustomerDetailsAsync(id, userId);
            if (model == null) return NotFound();
            return View(model);
        }

        public async Task<IActionResult> PrintInvoice(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();





            var model = await invoicesService.GetInvoiceDetailsAsync(id, userId);
            if (model == null) return NotFound();
            return View("~/Views/Invoices/Details.cshtml", model);
        }





        [HttpGet]
        public IActionResult Create() => View("Form", new CustomerFormViewModel { CustomerType = "Individual" });

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();




            var model = await customersService.GetCustomerForEditAsync(id, userId);
            if (model == null) return NotFound();
            return View("Form", model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(CustomerFormViewModel model)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();



            if (!ModelState.IsValid) return View("Form", model);

            try
            {
                await customersService.SaveCustomerAsync(model, userId);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Form", model);
            }
        }
    }
}