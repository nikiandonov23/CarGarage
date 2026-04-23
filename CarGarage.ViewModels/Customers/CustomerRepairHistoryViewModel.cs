using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarGarage.ViewModels.Customers
{
    public class CustomerRepairHistoryViewModel
    {
        public string CarModel { get; set; } = null!;
        public string RegistrationNumber { get; set; } = null!;
        public DateTime Date { get; set; }
        public string InvoiceNumber { get; set; } = null!;
        public string? Description { get; set; }
        public decimal TotalAmount { get; set; }
        public int InvoiceId { get; set; } 

    }
}
