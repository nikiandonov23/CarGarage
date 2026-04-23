using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarGarage.ViewModels.Customers
{
    public class CustomerCarViewModel
    {
        public int Id { get; set; }
        public string Make { get; set; } = null!;
        public string Model { get; set; } = null!;
        public string RegistrationNumber { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public int InvoicesCount { get; set; }
    }
}
