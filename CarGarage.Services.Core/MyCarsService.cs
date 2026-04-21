using CarGarage.Data;
using CarGarage.DataModels;
using CarGarage.ViewModels.Cars;
using CarGarage.ViewModels.Cars.Dropdowns;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Services.Core
{
    // Използване на Primary Constructor синтаксис
    public class MyCarsService(ApplicationDbContext context) : IMyCarsService
    {
        // 1. Връща всички коли на потребителя
        public async Task<IndexMyCarsViewModel> GetAllUserCarsAsync(string userId)
        {
            var cars = await context.UserCars
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

            return new IndexMyCarsViewModel { Cars = cars };
        }

        // 2. Взема данните за първоначално зареждане на формата (марките)
        public async Task<CreateCarViewModel> GetCreateCarViewModelAsync()
        {
            return new CreateCarViewModel
            {
                MakeList = await context.Makes
                    .Select(m => new CreateCarMakeDropDownViewModel
                    {
                        Id = m.Id,
                        Name = m.Name
                    })
                    .OrderBy(m => m.Name)
                    .ToListAsync()
            };
        }

        // 3. Взема моделите за конкретна марка (за AJAX)
        public async Task<IEnumerable<CreateCarModelDropDownViewModel>> GetModelsByMakeAsync(int makeId)
        {
            return await context.Models
                .Where(m => m.MakeId == makeId)
                .Select(m => new CreateCarModelDropDownViewModel
                {
                    Id = m.Id,
                    Name = m.Name
                })
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        // 4. Записва новата кола и я свързва с потребителя
        // Промени Task на Task<bool>
        public async Task<bool> AddCarToUserAsync(CreateCarViewModel model, string userId)
        {
            // 1. Проверка дали ТОЗИ потребител вече има такава кола (Рег. номер или VIN)
            var alreadyHasThisCar = await context.UserCars
                .AnyAsync(uc => uc.UserId == userId &&
                                (uc.Car.RegistrationNumber == model.RegistrationNumber ||
                                (uc.Car.Vin == model.Vin && model.Vin != "")));

            if (alreadyHasThisCar)
            {
                return false; // Вече я има, връщаме "неуспех"
            }

            // 2. Вземане на имена на марка и модел
            var makeObj = await context.Makes.FirstOrDefaultAsync(m => m.Id == model.MakeId);
            var modelObj = await context.Models.FirstOrDefaultAsync(m => m.Id == model.ModelId);

            // 3. Създаване на нов запис в Cars (за да е изолиран за този потребител)
            var car = new Car
            {
                RegistrationNumber = model.RegistrationNumber ?? "",
                Vin = model.Vin ?? "",
                ModelYear = model.ModelYear,
                Mileage = model.Mileage,
                ImageUrl = model.ImageUrl,
                Notes = model.Notes,
                Make = makeObj?.Name ?? "Unknown",
                Model = modelObj?.Name ?? "Unknown",
                AddedDate = DateTime.UtcNow
            };

            await context.Cars.AddAsync(car);
            await context.SaveChangesAsync();

            // 4. Свързване
            var userCar = new UserCars
            {
                UserId = userId,
                CarId = car.Id,
                AddedDate = DateTime.UtcNow
            };

            await context.UserCars.AddAsync(userCar);
            await context.SaveChangesAsync();

            return true; // Всичко е точно
        }
    }
}