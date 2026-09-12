using CarGarage.DataModels;
using CarGarage.ViewModels.Marketplace;

namespace CarGarage.Services.Core.Contracts
{
    public interface IOffersService
    {
        Task AddOfferAsync(
            int partForSaleId,
            decimal amount,
            string? message,
            DateTime? expiresAt,
            string senderId);

        Task AcceptOfferAsync(
            int offerId,
            string ownerId);

        Task RejectOfferAsync(
            int offerId,
            string ownerId);

        Task<IEnumerable<OfferListItemViewModel>> GetOffersForPartAsync(
            int partForSaleId,
            string ownerId);

        Task<IEnumerable<OfferListItemViewModel>> GetPendingOffersForOwnerAsync(string ownerId);

        Task<Offer?> GetByIdAsync(int offerId);

        Task MarkOfferPaidAsync(int offerId, string ownerId);

        Task MarkOfferNotPaidAsync(int offerId, string ownerId);
    }
}
