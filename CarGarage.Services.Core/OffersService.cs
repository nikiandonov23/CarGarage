using CarGarage.Data;
using CarGarage.DataModels;
using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Marketplace;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Services.Core
{
    public class OffersService(ApplicationDbContext context) : IOffersService
    {
        public async Task AddOfferAsync(int partForSaleId, decimal amount, string? message, DateTime? expiresAt, string senderId)
        {
            var offer = new Offer
            {
                PartForSaleId = partForSaleId,
                Amount = amount,
                Message = message,
                ExpiresAt = expiresAt,
                SenderId = senderId
            };

            await context.AddAsync(offer);
            await context.SaveChangesAsync();
        }

        public async Task RejectOfferAsync(int offerId, string ownerId)
        {
            var offer = await context.Offers
                .Include(o => o.PartForSale)
                .FirstOrDefaultAsync(o => o.Id == offerId);

            if (offer == null) return;

            if (offer.PartForSale == null || offer.PartForSale.OwnerId != ownerId) return;

            offer.Status = OfferStatus.Rejected;
            await context.SaveChangesAsync();
        }

        public async Task AcceptOfferAsync(int offerId, string ownerId)
        {
            var offer = await context.Offers
                .Include(o => o.PartForSale)
                .FirstOrDefaultAsync(o => o.Id == offerId);

            if (offer == null) return;

            // ensure the owner owns the part
            if (offer.PartForSale == null || offer.PartForSale.OwnerId != ownerId) return;

            offer.Status = OfferStatus.Accepted;
            if (offer.PartForSale != null)
            {
                offer.PartForSale.Status = "Pending";
            }

            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<OfferListItemViewModel>> GetOffersForPartAsync(int partForSaleId, string ownerId)
        {
            // only return offers if owner matches
            var part = await context.PartsForSale.FirstOrDefaultAsync(p => p.Id == partForSaleId && p.OwnerId == ownerId);
            if (part == null) return Enumerable.Empty<OfferListItemViewModel>();

            var offers = await context.Offers
                .Include(o => o.PartForSale)
                .Where(o => o.PartForSaleId == partForSaleId)
                .ToListAsync();

            return await MapToViewModelsAsync(offers);
        }

        public async Task<IEnumerable<OfferListItemViewModel>> GetPendingOffersForOwnerAsync(string ownerId)
        {
            // show both pending and accepted offers so owner can mark paid after acceptance
            // exclude offers for parts that were already sold
            var offers = await context.Offers
                .Include(o => o.PartForSale)
                .Where(o => (o.Status == OfferStatus.Pending || o.Status == OfferStatus.Accepted)
                            && o.PartForSale != null
                            && o.PartForSale.OwnerId == ownerId
                            && o.PartForSale.Status != "Sold")
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return await MapToViewModelsAsync(offers);
        }

        public async Task<Offer?> GetByIdAsync(int offerId)
        {
            return await context.Offers.Include(o => o.PartForSale).FirstOrDefaultAsync(o => o.Id == offerId);
        }

        public async Task MarkOfferPaidAsync(int offerId, string ownerId)
        {
            var offer = await context.Offers.Include(o => o.PartForSale).FirstOrDefaultAsync(o => o.Id == offerId);
            if (offer == null) return;
            if (offer.PartForSale == null || offer.PartForSale.OwnerId != ownerId) return;

            // mark part as Sold and remove from marketplace view
            offer.PartForSale.Status = "Sold";
            offer.Status = OfferStatus.Accepted; // ensure accepted
            await context.SaveChangesAsync();
        }

        public async Task MarkOfferNotPaidAsync(int offerId, string ownerId)
        {
            var offer = await context.Offers.Include(o => o.PartForSale)
                                             .FirstOrDefaultAsync(o => o.Id == offerId);
            if (offer == null) return;
            if (offer.PartForSale == null || offer.PartForSale.OwnerId != ownerId) return;

            // revert listing back to available so it appears on marketplace again
            offer.PartForSale.Status = "Available";

            // keep offer status as Accepted or adjust if desired
            offer.Status = OfferStatus.NotPaid;

            await context.SaveChangesAsync();
        }

        private async Task<IEnumerable<OfferListItemViewModel>> MapToViewModelsAsync(List<Offer> offers)
        {
            var senderIds = offers.Select(o => o.SenderId).Distinct().ToList();

            var senderNames = await context.Garages
                .Where(g => g.OwnerId != null && senderIds.Contains(g.OwnerId))
                .Select(g => new { g.OwnerId, g.Name })
                .ToDictionaryAsync(x => x.OwnerId!, x => x.Name);

            return offers.Select(o => new OfferListItemViewModel
            {
                Id = o.Id,
                PartName = o.PartForSale?.Name,
                SenderDisplayName = senderNames.TryGetValue(o.SenderId, out var name) && !string.IsNullOrWhiteSpace(name)
                    ? name!
                    : o.SenderId,
                Amount = o.Amount,
                Message = o.Message,
                Status = o.Status
            });
        }
    }
}