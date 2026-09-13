using System;

namespace CarGarage.ViewModels.Invoices
{
    public class InvoiceReportViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal CashRevenue { get; set; }
        public decimal CardRevenue { get; set; }
        public decimal BankTransferRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
        public bool IsGenerated { get; set; }
    }
}
