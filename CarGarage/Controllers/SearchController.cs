using Microsoft.AspNetCore.Authorization; // Задължителен using
using Microsoft.AspNetCore.Mvc;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Search;
using System.Security.Claims;

namespace CarGarage.Web.Controllers
{
    [Authorize] // <--- Само логнати потребители имат достъп до всичко в този контролер //test
    [Route("Search")]
    public class SearchController(ISearchService searchService) : BaseController
    {
        [HttpGet]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? searchTerm, string? customerName, int? makeId, int? modelId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!; // Вече може с усмивка да кажем, че не е null

            var viewModel = await searchService.GetSearchModelAsync(searchTerm, customerName, makeId, modelId, userId);

            return View(viewModel);
        }
    }
}