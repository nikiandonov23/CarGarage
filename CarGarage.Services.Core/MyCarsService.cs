using CarGarage.Data;
using CarGarage.DataModels;
using CarGarage.ViewModels.Cars;
using CarGarage.ViewModels.Cars.Dropdowns;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Services.Core
{
    public class MyCarsService(ApplicationDbContext context) : IMyCarsService
    {
        // връща списък с колите на юзърааа
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
                    ImageUrl = uc.Car.ImageUrl,
                    Notes = uc.Car.Notes,
                    AddedDate = uc.Car.AddedDate
                })
                .ToListAsync();

            return new IndexMyCarsViewModel { Cars = cars };
        }


        //връща криеййт моделчееее

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



        //връща моделитее за даден мейк
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

        public async Task<bool> AddCarToUserAsync(CreateCarViewModel model, string userId)
        {
            var alreadyHasThisCar = await context.UserCars
                .AnyAsync(uc => uc.UserId == userId &&
                                (uc.Car.RegistrationNumber == model.RegistrationNumber ||
                                (uc.Car.Vin == model.Vin && model.Vin != "")));

            if (alreadyHasThisCar)
            {
                return false;
            }

            var makeObj = await context.Makes.FirstOrDefaultAsync(m => m.Id == model.MakeId);
            var modelObj = await context.Models.FirstOrDefaultAsync(m => m.Id == model.ModelId);

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

            var userCar = new UserCars
            {
                UserId = userId,
                CarId = car.Id,
                AddedDate = DateTime.UtcNow
            };

            await context.UserCars.AddAsync(userCar);
            await context.SaveChangesAsync();

            return true;
        }

        // връща кола и пита сигурен ли съм ??? 
        public async Task<CarViewModel?> GetCarByIdAsync(int carId, string userId)
        {
            return await context.UserCars
                .Where(uc => uc.UserId == userId && uc.CarId == carId)
                .Select(uc => new CarViewModel
                {
                    Id = uc.Car.Id,
                    Make = uc.Car.Make,
                    Model = uc.Car.Model,
                    ModelYear = uc.Car.ModelYear,
                    RegistrationNumber = uc.Car.RegistrationNumber
                })
                .FirstOrDefaultAsync();
        }

        // ТРИЕеееееееееее
        public async Task<bool> DeleteCarForUserAsync(int carId, string userId)
        {
            var userCar = await context.UserCars
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CarId == carId);

            if (userCar == null) return false;

            // Премахваме връзката
            context.UserCars.Remove(userCar);

            // Изтриваме и самия автомобил от таблица Cars
            var car = await context.Cars.FindAsync(carId);
            if (car != null)
            {
                context.Cars.Remove(car);
            }

            await context.SaveChangesAsync();
            return true;
        }


        
        public async Task<CreateCarViewModel?> GetCarForEditAsync(int carId, string userId)
        {
            var car = await context.UserCars
                .Where(uc => uc.UserId == userId && uc.CarId == carId)
                .Select(uc => uc.Car)
                .FirstOrDefaultAsync();

            if (car == null) return null;

            var viewModel = new CreateCarViewModel
            {
                
                Vin = car.Vin,
                RegistrationNumber = car.RegistrationNumber,
                ModelYear = car.ModelYear,
                Mileage = car.Mileage,
                ImageUrl = car.ImageUrl,
                Notes = car.Notes,
                
                MakeList = await context.Makes
                    .Select(m => new CreateCarMakeDropDownViewModel { Id = m.Id, Name = m.Name })
                    .OrderBy(m => m.Name)
                    .ToListAsync()
            };

            
            var make = await context.Makes.FirstOrDefaultAsync(m => m.Name == car.Make);
            if (make != null)
            {
                viewModel.MakeId = make.Id;
                viewModel.ModelList = await GetModelsByMakeAsync(make.Id);

                var modelObj = await context.Models.FirstOrDefaultAsync(m => m.Name == car.Model && m.MakeId == make.Id);
                if (modelObj != null) viewModel.ModelId = modelObj.Id;
            }

            return viewModel;
        }

        public async Task<bool> UpdateCarAsync(CreateCarViewModel model, string userId)
        {
           
            var car = await context.UserCars
                .Where(uc => uc.UserId == userId && uc.CarId == model.Id)
                .Select(uc => uc.Car)
                .FirstOrDefaultAsync();

            if (car == null) return false;

            
            var makeObj = await context.Makes.FindAsync(model.MakeId);
            var modelObj = await context.Models.FindAsync(model.ModelId);

            
            car.RegistrationNumber = model.RegistrationNumber;
            car.Vin = model.Vin ?? "";
            car.ModelYear = model.ModelYear;
            car.Mileage = model.Mileage;
            car.ImageUrl = model.ImageUrl;
            car.Notes = model.Notes;
            car.Make = makeObj?.Name ?? "Unknown";
            car.Model = modelObj?.Name ?? "Unknown";

          
            await context.SaveChangesAsync();

            return true;
        }

        // ======================================================================== до тук бачка 
    }
}