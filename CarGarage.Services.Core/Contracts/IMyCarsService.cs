using CarGarage.ViewModels.MyCars;
using System.Threading.Tasks;

namespace CarGarage.Services.Core.Contracts
{
    public interface IMyCarsService
    {
        Task<IndexMyCarsViewModel> GetAllUserCarsAsync(string userId);

        Task CreateCarAsync(string userId, CreateCarViewModel model);

        // По-добро име за метода
        Task<CreateCarViewModel?> GetCarInfoByVinAsync(string vin);

        Task SaveCarAsync(CreateCarViewModel model);
    }
}