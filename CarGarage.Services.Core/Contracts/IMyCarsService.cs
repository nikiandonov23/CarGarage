using CarGarage.ViewModels.Cars;
using CarGarage.ViewModels.Cars.Dropdowns;

public interface IMyCarsService
{
    Task<IndexMyCarsViewModel> GetAllUserCarsAsync(string userId);

    // Добави този метод тук:
    Task<CreateCarViewModel> GetCreateCarViewModelAsync();

    Task<IEnumerable<CreateCarModelDropDownViewModel>> GetModelsByMakeAsync(int makeId);

    Task AddCarToUserAsync(CreateCarViewModel model, string userId);
}