using CarGarage.Services.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CarGarage.Web.Controllers
{
    public class CustomersController(ICustomersService customersService) : Controller
    {
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var model = await customersService.GetAllCustomersAsync(searchTerm);
            return View(model);
        }

        // Празен метод за Details сега ще го пълня
        public IActionResult Details(int id)
        {
            return View();
        }
    }
}