using CarGarage.ViewModels.Garage;

public interface IGarageService
{
    Task<bool> HasGarageAsync(string userId);
    Task<GarageViewModel?> GetGarageDetailsAsync(string userId);
    Task CreateGarageAsync(GarageViewModel model, string userId);
    Task UpdateGarageAsync(GarageViewModel model, string userId);
}