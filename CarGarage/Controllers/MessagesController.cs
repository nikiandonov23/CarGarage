using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Messages;
using CarGarage.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CarGarage.Controllers
{
    [Authorize]
    public class MessagesController : BaseController
    {
        private readonly IMessagesService _messagesService;
        private readonly IMarketplaceService _marketplaceService;
        private readonly IHubContext<Notifications.NotificationsHub> _hubContext;
        private readonly Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> _userManager;

        public MessagesController(IMessagesService messagesService, IMarketplaceService marketplaceService,
            IHubContext<Notifications.NotificationsHub> hubContext,
            Microsoft.AspNetCore.Identity.UserManager<Microsoft.AspNetCore.Identity.IdentityUser> userManager)
        {
            _messagesService = messagesService;
            _marketplaceService = marketplaceService;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? partId, string? receiverId, string? conversationId)
        {
            MessageFormModel model;

            if (!string.IsNullOrEmpty(receiverId))
            {
                model = new MessageFormModel { PartId = partId, ReceiverId = receiverId, ConversationId = conversationId };
            }
            else if (partId.HasValue)
            {
                var part = await _marketplaceService.GetByIdAsync(partId.Value);
                if (part == null) return NotFound();
                model = new MessageFormModel { PartId = partId, ReceiverId = part.OwnerId, ConversationId = conversationId };
            }
            else
            {
                return BadRequest();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MessageFormModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var senderId = GetUserId();
            if (string.IsNullOrEmpty(senderId)) return Unauthorized();

            await _messagesService.AddMessageAsync(senderId, model.ReceiverId, model.Content, model.PartId?.ToString());

            return RedirectToAction("Details", "Marketplace", new { id = model.PartId });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var inbox = await _messagesService.GetInboxAsync(userId);
            return View(inbox);
        }

        [HttpGet]
        public async Task<IActionResult> Outbox()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var outbox = await _messagesService.GetOutboxAsync(userId);
            return View(outbox);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var msg = await _messagesService.GetByIdAsync(id, userId);
            if (msg == null) return NotFound();

            if (msg.ReceiverId == userId && !msg.IsRead)
            {
                await _messagesService.MarkAsReadAsync(id, userId);
                // send updated unread count to the current user so badge refreshes
                try
                {
                    var unread = await _messagesService.GetUnreadCountAsync(userId);
                    await _hubContext.Clients.User(userId).SendAsync("UnreadCountUpdated", unread);
                }
                catch
                {
                    // ignore SignalR errors
                }
            }

            return View(msg);
        }

   
        // Pin/unpin handled in UI via future endpoint; temporarily not exposed to avoid interface mismatch.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _messagesService.DeleteAsync(id, userId);
            return RedirectToAction("Index");
        }
    }
}
