using CarGarage.ViewModels.Invoices;

namespace CarGarage.Services.Core.Contracts
{
    public interface IInvoicesService
    {
        Task<InvoiceFormModel> GetNewInvoiceModelAsync(int carId, string userId);

        Task<int> CreateInvoiceAsync(InvoiceFormModel model, string userId);

        Task<InvoiceFullViewModel> GetInvoiceDetailsAsync(int invoiceId, string userId);

        Task<IEnumerable<InvoiceFullViewModel>> GetAllUserInvoicesAsync(string userId);
    }
}