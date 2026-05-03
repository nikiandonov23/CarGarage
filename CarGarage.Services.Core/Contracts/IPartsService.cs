using CarGarage.ViewModels.Parts;

namespace CarGarage.Services.Core.Contracts
{
    public interface IPartsService
    {
        Task<IEnumerable<PartViewModel>> GetPartsByCarIdAsync(int carId, string userId);
        Task<PartViewModel?> GetPartByIdAsync(int id, string userId);
        Task<PartFormModel?> GetPartFormModelByIdAsync(int id, string userId);
        Task AddPartAsync(PartFormModel model, string userId);
        Task UpdatePartAsync(int id, PartFormModel model, string userId);
        Task DeleteAsync(int id, string userId);

        // Тези категории обикновено са общи, но за консистенция може да останат така
        Task<IEnumerable<PartCategoryViewModel>> GetCategoriesAsync();
    }
}