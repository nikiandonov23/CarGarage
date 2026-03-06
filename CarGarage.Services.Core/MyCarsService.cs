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
        public async Task CreateCarAsync(string userId, CreateCarViewModel model)
        {
            var car = new Car
            {
                Vin = model.Vin,
                Make = model.Make,
                Model = model.Model,
                ModelYear = model.ModelYear,
                
                
              
                
                RegistrationNumber = model.RegistrationNumber,
                Mileage = model.Mileage,
                ImageUrl = model.ImageUrl,
                AddedDate = DateTime.UtcNow
            };

            _context.Cars.Add(car);

            _context.UserCars.Add(new UserCars
            {
                UserId = userId,
                Car = car
            });

            await _context.SaveChangesAsync();
        }

        // Метод за извличане на информация по VIN от NHTSA API
        public async Task<CreateCarViewModel?> GetCarInfoByVinAsync(string vin)
        {
            if (string.IsNullOrEmpty(vin))
                return null;

            using var http = new HttpClient();
            var url = $"https://vpic.nhtsa.dot.gov/api/vehicles/decodevin/{vin}?format=json";
            var response = await http.GetStringAsync(url);
            var json = System.Text.Json.JsonDocument.Parse(response);
            var results = json.RootElement.GetProperty("Results");

            var model = new CreateCarViewModel();

            foreach (var r in results.EnumerateArray())
            {
                var name = r.GetProperty("Variable").GetString();
                var value = r.GetProperty("Value").GetString();

                switch (name)
                {
                    case "Make": model.Make = value ?? ""; break;
                    case "Model": model.Model = value ?? ""; break;
                    case "Model Year": model.ModelYear = int.TryParse(value, out var y) ? y : 0; break;
                    
                    
                  
                    
                }
            }

            model.IsPopulatedFromApi = true;
            return model;
        }
    }
}
