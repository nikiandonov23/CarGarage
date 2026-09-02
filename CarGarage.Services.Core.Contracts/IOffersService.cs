using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CarGarage.DataModels;

namespace CarGarage.Services.Core.Contracts
{
    public interface IOffersService
    {
        Task AddOfferAsync(int partForSaleId, decimal amount, string? message, DateTime? expiresAt, string senderId);
        Task RejectOfferAsync(int offerId, string ownerId);
        Task AcceptOfferAsync(int offerId, string ownerId);
        Task<IEnumerable<Offer>> GetOffersForPartAsync(int partForSaleId, string ownerId);
        Task<IEnumerable<Offer>> GetPendingOffersForOwnerAsync(string ownerId);
        Task<Offer?> GetByIdAsync(int offerId);
        Task MarkOfferPaidAsync(int offerId, string ownerId);
        // Revert a previously marked-as-paid offer — return the part to marketplace (Available)
        Task MarkOfferNotPaidAsync(int offerId, string ownerId);
    }
}
