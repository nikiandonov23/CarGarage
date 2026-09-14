using CarGarage.ViewModels.Marketplace;

namespace CarGarage.Services.Core.Contracts
{
    public interface IMarketplaceService
    {
        Task<MarketplaceIndexViewModel> GetMarketplaceAsync(string? searchTerm, string? city, int? categoryId, string? userId, string? ownerId = null);
        Task<PartForSaleViewModel?> GetByIdAsync(int id);
        Task<PartForSaleFormModel?> GetForEditAsync(int id, string userId);
        Task AddAsync(PartForSaleFormModel model, string userId);
        Task UpdateAsync(int id, PartForSaleFormModel model, string userId);
        Task DeleteAsync(int id, string userId);
        Task<IEnumerable<CarGarage.ViewModels.Parts.PartCategoryViewModel>> GetCategoriesAsync();
    }
}
