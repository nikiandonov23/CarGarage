namespace CarGarage.ViewModels.Invoices
{
    public class PartSelectionViewModel
    {
        public int PartId { get; set; }
        public string Description { get; set; } = null!;
        public decimal TotalPrice { get; set; }
        public bool IsSelected { get; set; }
    }
}