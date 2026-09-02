using CarGarage.Data;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Cars;
using CarGarage.ViewModels.Cars.Dropdowns;
using CarGarage.ViewModels.Search;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Services.Core
{
    public class SearchService(ApplicationDbContext context) : ISearchService
    {
        public async Task<SearchCarsViewModel> GetSearchModelAsync()
        {
            var makes = await context.Makes
                .Select(m => new CreateCarMakeDropDownViewModel
                {
                    Id = m.Id,
                    Name = m.Name
                })
                .OrderBy(m => m.Name)
                .ToListAsync();

            return new SearchCarsViewModel
            {
                Makes = makes
            };
        }

        public async Task<SearchCarsViewModel> GetSearchModelAsync(string? searchTerm, string? customerName, int? makeId, int? modelId, string? userId = null)
        {
            var viewModel = await GetSearchModelAsync();

            viewModel.SearchTerm = searchTerm;
            viewModel.CustomerName = customerName;
            viewModel.MakeId = makeId;
            viewModel.ModelId = modelId;

            // Зареждаме моделите, ако е избрана марка
            if (makeId.HasValue && makeId > 0)
            {
                var models = await context.Models
                    .Where(m => m.MakeId == makeId.Value)
                    .Select(m => new CreateCarModelDropDownViewModel
                    {
                        Id = m.Id,
                        Name = m.Name,
                        MakeId = m.MakeId
                    })
                    .OrderBy(m => m.Name)
                    .ToListAsync();

                viewModel.Models = models;
            }

            // Намираме резултатите само за твоя гараж
            viewModel.Results = (await SearchCarsAsync(searchTerm, customerName, makeId, modelId, userId)).ToList();

            return viewModel;
        }

        public async Task<IEnumerable<CarViewModel>> SearchCarsAsync(string? searchTerm, string? customerName, int? makeId, int? modelId, string? userId = null)
        {
            var query = context.Cars.AsNoTracking().AsQueryable();

            // СТРОГА ФИЛТРАЦИЯ ПО ГАРАЖ ПРЕЗ КЛИЕНТА: Взимаме само колите, чийто клиент принадлежи към твоя гараж
            if (!string.IsNullOrEmpty(userId))
            {
                var garageId = await context.Garages
                    .Where(g => g.OwnerId == userId)
                    .Select(g => (int?)g.Id)
                    .FirstOrDefaultAsync();

                if (garageId.HasValue && garageId.Value > 0)
                {
                    query = query.Where(c => c.Customer != null && c.Customer.GarageId == garageId.Value);
                }
                else
                {
                    // Ако потребителят няма регистриран гараж, връщаме празен списък
                    return Enumerable.Empty<CarViewModel>();
                }
            }
            else
            {
                // Ако няма логнат потребител, не връщаме резултати
                return Enumerable.Empty<CarViewModel>();
            }

            // Търсене по текст за рег. номер или VIN
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(c => (c.RegistrationNumber ?? string.Empty).ToLower().Contains(term) ||
                                         (c.Vin ?? string.Empty).ToLower().Contains(term));
            }

            // Филтър по марка
            if (makeId.HasValue && makeId > 0)
            {
                var makeName = await context.Makes
                    .Where(m => m.Id == makeId)
                    .Select(m => m.Name)
                    .FirstOrDefaultAsync();

                if (makeName != null)
                {
                    query = query.Where(c => c.Make == makeName);
                }
            }

            // Филтър по модел
            if (modelId.HasValue && modelId > 0)
            {
                var modelName = await context.Models
                    .Where(m => m.Id == modelId)
                    .Select(m => m.Name)
                    .FirstOrDefaultAsync();

                if (modelName != null)
                {
                    query = query.Where(c => c.Model == modelName);
                }
            }

            // Търсене по име на клиент (физическо или юридическо лице)
            if (!string.IsNullOrWhiteSpace(customerName))
            {
                var term = customerName.Trim().ToLower();
                query = query.Where(c => c.Customer != null && (
                    (c.Customer is IndividualCustomer && ((((IndividualCustomer)c.Customer).FirstName ?? string.Empty).ToLower().Contains(term) || (((IndividualCustomer)c.Customer).LastName ?? string.Empty).ToLower().Contains(term))) ||
                    (c.Customer is LegalEntityCustomer && (((LegalEntityCustomer)c.Customer).CompanyName ?? string.Empty).ToLower().Contains(term))
                ));
            }

            return await query
                .Select(c => new CarViewModel
                {
                    Id = c.Id,
                    Make = c.Make,
                    Model = c.Model,
                    ModelYear = c.ModelYear,
                    RegistrationNumber = c.RegistrationNumber,
                    Mileage = c.Mileage,
                    ImageUrl = c.ImageUrl,
                    Notes = c.Notes,
                    AddedDate = c.AddedDate
                })
                .ToListAsync();
        }
    }
}