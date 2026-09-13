using CarGarage.DataModels.Enums;
using CarGarage.ViewModels.Invoices;

namespace CarGarage.Services.Core.Contracts
{
    public interface IInvoicesService
    {
        Task<InvoiceFormModel> GetNewInvoiceModelAsync(int carId, string userId);

        Task<int> CreateInvoiceAsync(InvoiceFormModel model, string userId);

        Task<InvoiceFullViewModel> GetInvoiceDetailsAsync(int invoiceId, string userId);

        Task<IEnumerable<InvoiceFullViewModel>> GetAllUserInvoicesAsync(
            string userId, 
            string? status = null, 
            PaymentMethod? paymentMethod = null, 
            string? clientSearch = null, 
            string? carSearch = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? sortBy = null,
            bool? isAsc = null);

        Task<bool> AnnulInvoiceAsync(int invoiceId, string userId);

        Task<InvoiceReportViewModel> GetRevenueReportAsync(string userId, DateTime? startDate, DateTime? endDate);
    }
}