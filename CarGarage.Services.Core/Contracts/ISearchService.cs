using CarGarage.ViewModels.Search;
using CarGarage.ViewModels.Cars;

namespace CarGarage.Services.Core.Contracts
{
    public interface ISearchService
    {
        // гласи марките за дропдауна
        Task<SearchCarsViewModel> GetSearchModelAsync();

        Task<SearchCarsViewModel> GetSearchModelAsync(string? searchTerm, string? customerName, int? makeId, int? modelId);

        // филтърче
        Task<IEnumerable<CarViewModel>> SearchCarsAsync(string? searchTerm, string? customerName, int? makeId, int? modelId);
    }
}