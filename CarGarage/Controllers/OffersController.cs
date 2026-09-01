using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Marketplace;
using CarGarage.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CarGarage.Controllers
{
    [Authorize]
    public class OffersController : BaseController
    {
        private readonly IOffersService _offersService;
        private readonly IMessagesService _messagesService;
        private readonly Microsoft.AspNetCore.SignalR.IHubContext<Notifications.NotificationsHub> _hubContext;

        private readonly IMarketplaceService _marketplaceService;

        public OffersController(IOffersService offersService, IMessagesService messagesService,
            Microsoft.AspNetCore.SignalR.IHubContext<Notifications.NotificationsHub> hubContext,
            IMarketplaceService marketplaceService)
        {
            _offersService = offersService;
            _messagesService = messagesService;
            _hubContext = hubContext;
            _marketplaceService = marketplaceService;
        }

        [HttpGet]
        public IActionResult MakeOffer(int partId)
        {
            var model = new OfferFormModel { PartId = partId };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeOffer(OfferFormModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _offersService.AddOfferAsync(model.PartId, model.Amount, model.Message, model.ExpiresAt, userId);

            // notify owner: get part details to find owner id
            var part = await _marketplaceService.GetByIdAsync(model.PartId);
            if (part != null && !string.IsNullOrEmpty(part.OwnerId))
            {
                // send a system message to the owner
                var systemMsg = $"You have a new offer ({model.Amount.ToString("N2")}) for part '{part.Name}'";
                await _messagesService.AddMessageAsync(userId, part.OwnerId, systemMsg, part.Id.ToString());

                // send SignalR unread count update to owner
                try
                {
                    var unread = await _messagesService.GetUnreadCountAsync(part.OwnerId);
                    await _hubContext.Clients.User(part.OwnerId).SendAsync("UnreadCountUpdated", unread);
                }
                catch { }
            }

            return RedirectToAction("Details", "Marketplace", new { id = model.PartId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int offerId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _offersService.AcceptOfferAsync(offerId, userId);
            // redirect back to marketplace or offers list
            return RedirectToAction("Index", "Marketplace");
        }
    }
}
