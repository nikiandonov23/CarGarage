using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarGarage.ViewModels.Cars.Dropdowns
{
    public class CreateCarModelDropDownViewModel
    {

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int MakeId { get; set; }
    }
}
