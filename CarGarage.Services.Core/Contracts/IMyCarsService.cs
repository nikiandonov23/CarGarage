using CarGarage.ViewModels.Cars;
using CarGarage.ViewModels.Cars.Dropdowns;

public interface IMyCarsService
{
    Task<IndexMyCarsViewModel> GetAllUserCarsAsync(string userId);

    Task<CreateCarViewModel> GetCreateCarViewModelAsync();

    Task<IEnumerable<CreateCarModelDropDownViewModel>> GetModelsByMakeAsync(int makeId);

    Task<bool> AddCarToUserAsync(CreateCarViewModel model, string userId);


    
    Task<CarViewModel?> GetCarByIdAsync(int carId, string userId);
    Task<bool> DeleteCarForUserAsync(int carId, string userId);

   

    Task<CreateCarViewModel?> GetCarForEditAsync(int carId, string userId);
    Task<bool> UpdateCarAsync(CreateCarViewModel model, string userId);

   
}