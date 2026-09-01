using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarGarage.Data;
using CarGarage.DataModels;
using CarGarage.Services.Core.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CarGarage.Services.Core
{
    public class OffersService : IOffersService
    {
        private readonly ApplicationDbContext _context;

        public OffersService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddOfferAsync(int partForSaleId, decimal amount, string? message, DateTime? expiresAt, string senderId)
        {
            var offer = new CarGarage.DataModels.Offer
            {
                PartForSaleId = partForSaleId,
                Amount = amount,
                Message = message,
                ExpiresAt = expiresAt,
                SenderId = senderId
            };

            await _context.AddAsync(offer);
            await _context.SaveChangesAsync();
        }

        public async Task AcceptOfferAsync(int offerId, string ownerId)
        {
            var offer = await _context.Offers
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

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CarGarage.DataModels.Offer>> GetOffersForPartAsync(int partForSaleId, string ownerId)
        {
            // only return offers if owner matches
            var part = await _context.PartsForSale.FirstOrDefaultAsync(p => p.Id == partForSaleId && p.OwnerId == ownerId);
            if (part == null) return Enumerable.Empty<CarGarage.DataModels.Offer>();

            return await _context.Offers.Where(o => o.PartForSaleId == partForSaleId).ToListAsync();
        }
    }
}
