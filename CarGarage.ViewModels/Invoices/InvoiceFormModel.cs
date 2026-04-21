using System.ComponentModel.DataAnnotations;

namespace CarGarage.ViewModels.Invoices
{
    public class InvoiceFormModel
    {
        public int CarId { get; set; }
        public string CarInfo { get; set; } = null!;

        [Display(Name = "Труд (цена на час)")]
        [Range(0, 10000)]
        public decimal LaborPricePerHour { get; set; }

        [Display(Name = "Работни часове")]
        [Range(0, 500)]
        public double LaborHours { get; set; }

        [Display(Name = "ДДС (%)")]
        public decimal TaxPercentage { get; set; } = 20;

        [Display(Name = "Бележки")]
        public string? Notes { get; set; }

        public List<PartSelectionViewModel> AvailableParts { get; set; } = new();
    }
}