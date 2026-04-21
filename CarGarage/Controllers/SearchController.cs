using Microsoft.AspNetCore.Mvc;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Search;

namespace CarGarage.Web.Controllers
{
    // Този атрибут гарантира, че URL адресът винаги ще бъде /Search
    [Route("Search")]
    public class SearchController(ISearchService searchService, IMyCarsService myCarsService) : BaseController
    {
        // Достъпно чрез /Search или /Search/Index
        [HttpGet]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? searchTerm, int? makeId, int? modelId)
        {
            // 1. Първоначално зареждане на модела с марките
            var viewModel = await searchService.GetSearchModelAsync();

            // 2. Запазваме филтрите, за да стоят попълнени в инпутите след натискане на бутона
            viewModel.SearchTerm = searchTerm;
            viewModel.MakeId = makeId;
            viewModel.ModelId = modelId;

            // 3. Ако потребителят е избрал марка, зареждаме моделите й за падащото меню
            if (makeId.HasValue && makeId > 0)
            {
                viewModel.Models = await myCarsService.GetModelsByMakeAsync(makeId.Value);
            }

            // 4. Извличаме резултатите от търсенето
            viewModel.Results = await searchService.SearchCarsAsync(searchTerm, makeId, modelId);

            return View(viewModel);
        }
    }
}