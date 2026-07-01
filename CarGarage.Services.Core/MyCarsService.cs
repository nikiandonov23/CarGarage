using CarGarage.Data;
using CarGarage.DataModels;
using CarGarage.ViewModels.Cars;
using CarGarage.ViewModels.Cars.Dropdowns;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Services.Core
{
    public class MyCarsService(ApplicationDbContext context) : IMyCarsService
    {
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

        public async Task<CreateCarViewModel> GetCreateCarViewModelAsync(string userId)
        {
            // 1. Намираме гаража на потребителя, за да филтрираме само неговите клиенти
            var userGarage = await context.Set<Garage>().FirstOrDefaultAsync(g => g.OwnerId == userId);
            int garageId = userGarage?.Id ?? 0;

            // 2. Взимаме само клиентите, които принадлежат на този гараж
            var customers = await context.Set<Customer>()
                .Where(c => c.GarageId == garageId)
                .Select(c => new CreateCarCustomerDropDownViewModel
                {
                    Id = c.Id,
                    Name = c is IndividualCustomer
                        ? ((IndividualCustomer)c).FirstName + " " + ((IndividualCustomer)c).LastName
                        : ((LegalEntityCustomer)c).CompanyName
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return new CreateCarViewModel
            {
                MakeList = await context.Makes
                    .Select(m => new CreateCarMakeDropDownViewModel { Id = m.Id, Name = m.Name })
                    .OrderBy(m => m.Name)
                    .ToListAsync(),
                CustomerList = customers
            };
        }

        public async Task<IEnumerable<CreateCarModelDropDownViewModel>> GetModelsByMakeAsync(int makeId)
        {
            return await context.Models
                .Where(m => m.MakeId == makeId)
                .Select(m => new CreateCarModelDropDownViewModel { Id = m.Id, Name = m.Name })
                .OrderBy(m => m.Name)
                .ToListAsync();
        }

        public async Task<bool> AddCarToUserAsync(CreateCarViewModel model, string userId)
        {
            // 1. Проверка за дублиран автомобил за ТОЗИ потребител
            var alreadyHasThisCar = await context.UserCars
                .AnyAsync(uc => uc.UserId == userId &&
                                (uc.Car.RegistrationNumber == model.RegistrationNumber ||
                                (uc.Car.Vin == model.Vin && model.Vin != "" && model.Vin != null)));

            if (alreadyHasThisCar) return false;

            // 2. Намиране на ОРИГИНАЛНИЯ гараж на текущия потребител
            var userGarage = await context.Set<Garage>().FirstOrDefaultAsync(g => g.OwnerId == userId);

            // Ако потребителят няма гараж, не можем да добавим клиент/кола към нищо
            if (userGarage == null) return false;

            int garageId = userGarage.Id;
            int? finalCustomerId = null;

            // 3. Обработка на Клиента - Съществуващ или Нов
            if (!model.IsNewCustomer)
            {
                if (!model.CustomerId.HasValue) return false;
                finalCustomerId = model.CustomerId.Value;
            }
            else
            {
                if (model.NewCustomerType == "Individual")
                {
                    if (string.IsNullOrWhiteSpace(model.NewFirstName) ||
                        string.IsNullOrWhiteSpace(model.NewLastName) ||
                        string.IsNullOrWhiteSpace(model.NewCustomerAddress) ||
                        string.IsNullOrWhiteSpace(model.NewCustomerCity) ||
                        string.IsNullOrWhiteSpace(model.NewCustomerEmail) ||
                        string.IsNullOrWhiteSpace(model.NewCustomerPhoneNumber) ||
                        string.IsNullOrWhiteSpace(model.NewCustomerEgn))
                        return false;

                    var newIndividual = new IndividualCustomer
                    {
                        FirstName = model.NewFirstName.Trim(),
                        LastName = model.NewLastName.Trim(),
                        Egn = model.NewCustomerEgn.Trim(),
                        Email = model.NewCustomerEmail.Trim(),
                        PhoneNumber = model.NewCustomerPhoneNumber.Trim(),
                        Address = model.NewCustomerAddress.Trim(),
                        City = model.NewCustomerCity.Trim(),
                        GarageId = garageId
                    };
                    await context.Set<Customer>().AddAsync(newIndividual);
                    await context.SaveChangesAsync();
                    finalCustomerId = newIndividual.Id;
                }
                else if (model.NewCustomerType == "LegalEntity")
                {
                    if (string.IsNullOrWhiteSpace(model.NewCompanyName) ||
                        string.IsNullOrWhiteSpace(model.NewCustomerVatNumber) ||
                        string.IsNullOrWhiteSpace(model.NewCustomerResponsiblePerson) ||
                        string.IsNullOrWhiteSpace(model.NewCustomerAddress) ||
                        string.IsNullOrWhiteSpace(model.NewCustomerCity) ||
                        string.IsNullOrWhiteSpace(model.NewCustomerEmail) ||
                        string.IsNullOrWhiteSpace(model.NewCustomerPhoneNumber))
                        return false;

                    var newCompany = new LegalEntityCustomer
                    {
                        CompanyName = model.NewCompanyName.Trim(),
                        VatNumber = model.NewCustomerVatNumber.Trim(),
                        IsVatRegistered = model.NewCustomerIsVatRegistered,
                        ResponsiblePerson = model.NewCustomerResponsiblePerson.Trim(),
                        Email = model.NewCustomerEmail.Trim(),
                        PhoneNumber = model.NewCustomerPhoneNumber.Trim(),
                        Address = model.NewCustomerAddress.Trim(),
                        City = model.NewCustomerCity.Trim(),
                        GarageId = garageId
                    };
                    await context.Set<Customer>().AddAsync(newCompany);
                    await context.SaveChangesAsync();
                    finalCustomerId = newCompany.Id;
                }
            }

            var makeObj = await context.Makes.FirstOrDefaultAsync(m => m.Id == model.MakeId);
            var modelObj = await context.Models.FirstOrDefaultAsync(m => m.Id == model.ModelId);

            // 4. Създаване на колата
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
                AddedDate = DateTime.UtcNow,
                CustomerId = finalCustomerId
            };

            await context.Cars.AddAsync(car);
            await context.SaveChangesAsync();

            // 5. Обвързване на колата с ТЕКУЩИЯ потребител
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

        public async Task<bool> DeleteCarForUserAsync(int carId, string userId)
        {
            var userCar = await context.UserCars
                .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CarId == carId);

            if (userCar == null) return false;

            context.UserCars.Remove(userCar);

            var car = await context.Cars.FindAsync(carId);
            if (car != null) context.Cars.Remove(car);

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

            var userGarage = await context.Set<Garage>().FirstOrDefaultAsync(g => g.OwnerId == userId);
            int garageId = userGarage?.Id ?? 0;

            var customers = await context.Set<Customer>()
                .Where(c => c.GarageId == garageId)
                .Select(c => new CreateCarCustomerDropDownViewModel
                {
                    Id = c.Id,
                    Name = c is IndividualCustomer
                        ? ((IndividualCustomer)c).FirstName + " " + ((IndividualCustomer)c).LastName
                        : ((LegalEntityCustomer)c).CompanyName
                })
                .OrderBy(c => c.Name)
                .ToListAsync();

            var viewModel = new CreateCarViewModel
            {
                Id = car.Id,
                Vin = car.Vin,
                RegistrationNumber = car.RegistrationNumber,
                ModelYear = car.ModelYear,
                Mileage = car.Mileage,
                ImageUrl = car.ImageUrl,
                Notes = car.Notes,
                CustomerId = car.CustomerId,

                MakeList = await context.Makes
                    .Select(m => new CreateCarMakeDropDownViewModel { Id = m.Id, Name = m.Name })
                    .OrderBy(m => m.Name)
                    .ToListAsync(),
                CustomerList = customers
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
            car.CustomerId = model.CustomerId;

            await context.SaveChangesAsync();
            return true;
        }
    }
}