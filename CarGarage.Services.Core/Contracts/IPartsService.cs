using CarGarage.ViewModels.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarGarage.Services.Core.Contracts
{
    public interface IPartsService
    {
        Task<IEnumerable<PartViewModel>> GetPartsByCarIdAsync(int carId);

        Task<PartViewModel?> GetPartByIdAsync(int id);

        Task<PartFormModel?> GetPartFormModelByIdAsync(int id); // За Edit

        Task AddPartAsync(PartFormModel model);

        Task UpdatePartAsync(int id, PartFormModel model); // За Editaa

        Task<IEnumerable<PartCategoryViewModel>> GetCategoriesAsync();

        Task DeleteAsync(int id);
    }
}
