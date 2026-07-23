using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Search;

namespace CarGarage.Web.Controllers
{
    [Authorize]
    [Route("Search")]
    public class SearchController : BaseController
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? searchTerm, string? customerName, int? makeId, int? modelId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var viewModel = await _searchService.GetSearchModelAsync(searchTerm, customerName, makeId, modelId, userId);
            return View(viewModel);
        }
    }
}
