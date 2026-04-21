using CarGarage.ViewModels.Search;
using CarGarage.ViewModels.Cars;

namespace CarGarage.Services.Core.Contracts
{
    public interface ISearchService
    {
        // гласи марките за дропдауна
        Task<SearchCarsViewModel> GetSearchModelAsync();

        // филтърче
        Task<IEnumerable<CarViewModel>> SearchCarsAsync(string? searchTerm, int? makeId, int? modelId);
    }
}