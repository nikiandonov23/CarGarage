using Microsoft.AspNetCore.Mvc;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Search;

namespace CarGarage.Web.Controllers
{
    
    [Route("Search")]
    public class SearchController(ISearchService searchService, IMyCarsService myCarsService) : BaseController
    {
        
        [HttpGet]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? searchTerm, string? customerName, int? makeId, int? modelId)
        {
            
            var viewModel = await searchService.GetSearchModelAsync();



            viewModel.SearchTerm = searchTerm;
            viewModel.CustomerName = customerName;
            viewModel.MakeId = makeId;
            viewModel.ModelId = modelId;

            if (makeId.HasValue && makeId > 0)
            {
                viewModel.Models = await myCarsService.GetModelsByMakeAsync(makeId.Value);
            }

            viewModel.Results = await searchService.SearchCarsAsync(searchTerm, customerName, makeId, modelId);

            return View(viewModel);

        }
    }
}