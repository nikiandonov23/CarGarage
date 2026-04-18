using CarGarage.Data;
using CarGarage.DataModels;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.MyCars;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Services.Core
{
    public class MyCarsService : IMyCarsService
    {
        private readonly ApplicationDbContext _context;

        public MyCarsService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Връща всички коли на потребителя
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

            return new IndexMyCarsViewModel { Cars = cars };
        }




        // 2. Основна логика за създаване на автомобил
        public async Task CreateCarAsync(string userId, CreateCarViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.RegistrationNumber))
                throw new ArgumentException("Регистрационният номер е задължителен!");

            // Нормализираме VIN (ако е въведен)
            string? normalizedVin = string.IsNullOrWhiteSpace(model.Vin)
                ? null
                : model.Vin.Trim().ToUpper();

            // Проверяваме дали колата вече съществува по VIN
            Car? existingCar = null;
            if (!string.IsNullOrEmpty(normalizedVin))
            {
                existingCar = await _context.Cars
                    .FirstOrDefaultAsync(c => c.Vin == normalizedVin);
            }

            if (existingCar != null)
            {
                // Обновяваме съществуващата кола
                existingCar.Make = model.Make;
                existingCar.Model = model.Model;
                existingCar.ModelYear = model.ModelYear;
                existingCar.RegistrationNumber = model.RegistrationNumber;
                existingCar.Mileage = model.Mileage;
                existingCar.ImageUrl = model.ImageUrl;
                existingCar.Notes = model.Notes ?? "";

                await SaveCarAsync(model); // използваме Save метода
            }
            else
            {
                // Създаваме нова кола
                var newCar = new Car
                {
                    Vin = normalizedVin ?? "",
                    Make = model.Make,
                    Model = model.Model,
                    ModelYear = model.ModelYear,
                    RegistrationNumber = model.RegistrationNumber,
                    Mileage = model.Mileage,
                    ImageUrl = model.ImageUrl,
                    Notes = model.Notes ?? "",
                    AddedDate = DateTime.UtcNow
                };

                _context.Cars.Add(newCar);
                await _context.SaveChangesAsync();

                // Записваме връзката с потребителя
                var userCar = new UserCars
                {
                    UserId = userId,
                    CarId = newCar.Id,
                    AddedDate = DateTime.UtcNow
                };

                _context.UserCars.Add(userCar);
                await _context.SaveChangesAsync();
            }
        }






        // 3. Метод за автоматично попълване от VIN (NHTSA API)
        public async Task<CreateCarViewModel?> GetCarInfoByVinAsync(string vin)
        {
            if (string.IsNullOrWhiteSpace(vin))
                return null;

            vin = vin.Trim().ToUpper();

            var url = $"https://vpic.nhtsa.dot.gov/api/vehicles/DecodeVinValues/{vin}?format=json";

            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

                var response = await http.GetStringAsync(url);
                var json = System.Text.Json.JsonDocument.Parse(response);

                if (!json.RootElement.TryGetProperty("Results", out var results) || results.GetArrayLength() == 0)
                    return null;

                var result = results[0];

                var model = new CreateCarViewModel
                {
                    Vin = vin,
                    IsPopulatedFromApi = true
                };

                if (result.TryGetProperty("Make", out var makeProp))
                    model.Make = makeProp.GetString() ?? "";

                if (result.TryGetProperty("Model", out var modelProp))
                    model.Model = modelProp.GetString() ?? "";
                else if (result.TryGetProperty("Series", out var seriesProp))
                    model.Model = seriesProp.GetString() ?? "";

                if (result.TryGetProperty("ModelYear", out var yearProp))
                {
                    string? yearStr = yearProp.GetString();
                    if (int.TryParse(yearStr, out int year) && year >= 1900)
                        model.ModelYear = year;
                }

                return model;
            }
            catch
            {
                return null;
            }
        }





        // 4. Отделен метод само за запис в базата (Save)
        public async Task SaveCarAsync(CreateCarViewModel model)
        {
            // Този метод може да се използва и за обновяване, ако искаш по-късно
            // Засега го оставяме празен или с минимална логика
            await Task.CompletedTask; // placeholder
        }
    }
}