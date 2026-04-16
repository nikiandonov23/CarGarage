using CarGarage.Data;
using CarGarage.DataModels;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.MyCars;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Services.Core
{
    public class MyCarsService(ApplicationDbContext context) : IMyCarsService
    {
        private readonly ApplicationDbContext _context = context;

    

        // Метод за връщане на всички коли на потребителя
        public async Task<IndexMyCarsViewModel> GetAllUserCarsAsync(string userId)
        {
            var cars = await _context.UserCars
                .Where(uc => uc.UserId == userId)
                .Select(uc => new CarViewModel
                {
                    Id = uc.Car.Id,
                    Make = uc.Car.Make,
                    Model = uc.Car.Model,
                    ModelYear = uc.Car.ModelYear,




                    RegistrationNumber = uc.Car.RegistrationNumber,
                    Mileage = uc.Car.Mileage,
                    ImageUrl = uc.Car.ImageUrl
                })
                .ToListAsync();

            return new IndexMyCarsViewModel
            {
                Cars = cars
            };
        }


        // Метод за създаване на нов автомобил
        public Task CreateCarAsync(string userId, CreateCarViewModel model)
        {
            throw new NotImplementedException();
        }


        //Метод за автоматично попълване на данни за автомобил с аякс заявка към външно API по Маке>Model list for this make>ModelYear>
        public Task<CreateCarViewModel?> GetCarInfo(string vin)
        {
            throw new NotImplementedException();
        }

        public Task SaveCarAsync(CreateCarViewModel model)
        {
            throw new NotImplementedException();
        }



        




    }
}
