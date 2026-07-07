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
        // моделите за дропдайна майко
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

        public async Task<SearchCarsViewModel> GetSearchModelAsync(string? searchTerm, string? customerName, int? makeId, int? modelId)
        {
            // Start from base model with makes
            var viewModel = await GetSearchModelAsync();

            // set filter values
            viewModel.SearchTerm = searchTerm;
            viewModel.CustomerName = customerName;
            viewModel.MakeId = makeId;
            viewModel.ModelId = modelId;

            // load models for selected make
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

            // search results
            viewModel.Results = (await SearchCarsAsync(searchTerm, customerName, makeId, modelId)).ToList();

            return viewModel;
        }

        
        public async Task<IEnumerable<CarViewModel>> SearchCarsAsync(string? searchTerm, string? customerName, int? makeId, int? modelId)
        {
            //лепя филтрите към осн. заявка дето зема вс коли 
            var query = context.Cars.AsNoTracking().AsQueryable();

            // търсене по текст за вина или рег.номрра
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(c => (c.RegistrationNumber ?? string.Empty).ToLower().Contains(term) ||
                                         (c.Vin ?? string.Empty).ToLower().Contains(term));
            }

            // пресявам ги по марката 
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

            // пресявам ги по моделаа
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

            // търсене по име на клиент (физическо или юридическо лице)
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