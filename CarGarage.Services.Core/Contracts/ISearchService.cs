using CarGarage.ViewModels.Search;
using CarGarage.ViewModels.Cars;

namespace CarGarage.Services.Core.Contracts
{
    public interface ISearchService
    {
        // Гласи марките за дропдауна (базов модел)
        Task<SearchCarsViewModel> GetSearchModelAsync();

        // userId вече е задължителен параметър (премахната е въпросителната)
        Task<SearchCarsViewModel> GetSearchModelAsync(string? searchTerm, string? customerName, int? makeId, int? modelId, string userId);

        // userId е задължителен и тук
        Task<IEnumerable<CarViewModel>> SearchCarsAsync(string? searchTerm, string? customerName, int? makeId, int? modelId, string userId);
    }
}