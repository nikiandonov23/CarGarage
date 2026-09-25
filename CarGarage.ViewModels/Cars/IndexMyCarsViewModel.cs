using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CarGarage.ViewModels.Cars.Dropdowns;

namespace CarGarage.ViewModels.Cars
{
    public class IndexMyCarsViewModel
    {
        public List<CarViewModel> Cars { get; set; } = new();

        public string? SearchTerm { get; set; }
        public string? CustomerName { get; set; }
        public int? MakeId { get; set; }
        public int? ModelId { get; set; }

        public IEnumerable<CreateCarMakeDropDownViewModel> Makes { get; set; } = new List<CreateCarMakeDropDownViewModel>();
        public IEnumerable<CreateCarModelDropDownViewModel> Models { get; set; } = new List<CreateCarModelDropDownViewModel>();
    }
}
