using Microsoft.AspNetCore.Mvc;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Search;

namespace CarGarage.Web.Controllers
{

    [Route("Search")]
    public class SearchController(ISearchService searchService) : BaseController
    {

        [HttpGet]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? searchTerm, string? customerName, int? makeId, int? modelId) { var viewModel = await searchService.GetSearchModelAsync(searchTerm, customerName, makeId, modelId); return View(viewModel); }
    }
}