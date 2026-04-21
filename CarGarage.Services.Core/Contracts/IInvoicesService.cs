using CarGarage.ViewModels.Invoices;

namespace CarGarage.Services.Core.Contracts
{
    public interface IInvoicesService
    {
        Task<InvoiceFormModel> GetNewInvoiceModelAsync(int carId);
        Task<int> CreateInvoiceAsync(InvoiceFormModel model);
        Task<InvoiceFullViewModel> GetInvoiceDetailsAsync(int invoiceId);


        Task<IEnumerable<InvoiceFullViewModel>> GetAllUserInvoicesAsync(string userId);
    }
}