namespace CarGarage.ViewModels.Invoices
{
    public class InvoiceFullViewModel
    {

        public int Id { get; set; }

        public string InvoiceNumber { get; set; } = null!;
        public DateTime IssuedDate { get; set; }
        public string CarInfo { get; set; } = null!;
        public string Vin { get; set; } = null!;
        public string RegNumber { get; set; } = null!;
        public List<InvoicePartViewModel> Parts { get; set; } = new();
        public decimal LaborTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public string? Notes { get; set; }


        public int CarId { get; set; }
    }
}