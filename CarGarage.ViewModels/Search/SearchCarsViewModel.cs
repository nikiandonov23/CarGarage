using CarGarage.ViewModels.Cars;
using CarGarage.ViewModels.Cars.Dropdowns;

namespace CarGarage.ViewModels.Search
{
    public class SearchCarsViewModel
    {
        
        public string? SearchTerm { get; set; } 
        public int? MakeId { get; set; }
        public int? ModelId { get; set; }

        
        public IEnumerable<CarViewModel> Results { get; set; } = new List<CarViewModel>();

        
        public IEnumerable<CreateCarMakeDropDownViewModel> Makes { get; set; } = new List<CreateCarMakeDropDownViewModel>();
        public IEnumerable<CreateCarModelDropDownViewModel> Models { get; set; } = new List<CreateCarModelDropDownViewModel>();
    }
}