using CarGarage.DataModels;

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

        Task<IEnumerable<Offer>> GetOffersForPartAsync(
            int partForSaleId,
            string ownerId);
    }
}
