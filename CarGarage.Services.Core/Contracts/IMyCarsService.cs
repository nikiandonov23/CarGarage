using CarGarage.ViewModels.MyCars;
using System.Threading.Tasks;

namespace CarGarage.Services.Core.Contracts
{
    public interface IMyCarsService
    {
        // Връща всички коли на потребителя
        Task<IndexMyCarsViewModel> GetAllUserCarsAsync(string userId);

        // Създава нов автомобил
        Task CreateCarAsync(string userId, CreateCarViewModel model);

        // Връща информация за автомобил по VIN (NHTSA API)
        Task<CreateCarViewModel?> GetCarInfo(string vin);


        //Метод за сейв на нов автомобил в базата данни
        Task SaveCarAsync(CreateCarViewModel model);
    }
}
