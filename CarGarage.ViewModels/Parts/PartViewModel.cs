using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarGarage.ViewModels.Parts
{
    public class PartViewModel
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime DateAdded { get; set; }
        public string CarInfo { get; set; } = null!; // Марка plus Модел за фактурата

        public int? InvoiceId { get; set; }
    }
}
