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

            // Use provided ConversationId when available, otherwise use PartId as a fallback conversation key
            var conversationKey = model.ConversationId ?? model.PartId?.ToString();
            await _messagesService.AddMessageAsync(senderId, model.ReceiverId, model.Content, conversationKey);

            // Redirect back to marketplace details when message is about a part
            if (model.PartId.HasValue)
            {
                return RedirectToAction("Details", "Marketplace", new { id = model.PartId });
            }

            // Otherwise go back to inbox where the conversation will appear
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var inbox = (await _messagesService.GetInboxAsync(userId)).ToList();

            // group by conversation id or sender id
            var groups = inbox.GroupBy(m => m.ConversationId ?? m.SenderId)
                .Select(g => new CarGarage.ViewModels.Messages.ConversationSummaryViewModel
                {
                    ConversationId = g.Key,
                    LatestMessageId = g.OrderByDescending(x => x.SentAt).First().Id,
                    OtherUserId = g.First().SenderId,
                    LastMessage = g.OrderByDescending(x => x.SentAt).First().Content,
                    LastSentAt = g.OrderByDescending(x => x.SentAt).First().SentAt,
                    UnreadCount = g.Count(x => !x.IsRead && x.ReceiverId == userId),
                    IsPinned = g.Any(x => x.IsPinned)
                })
                .OrderByDescending(g => g.LastSentAt)
                .ToList();

            // resolve display names
            foreach (var conv in groups)
            {
                var usr = await _userManager.FindByIdAsync(conv.OtherUserId);
                conv.OtherUserName = usr?.UserName ?? conv.OtherUserId;
            }

            return View(groups);
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
            // fetch conversation messages (by conversation id or message id)
            var conversation = await _messagesService.GetConversationAsync(id, userId);
            if (conversation == null || !conversation.Any()) return NotFound();

            // mark unread messages in this conversation as read
            foreach (var m in conversation.Where(x => x.ReceiverId == userId && !x.IsRead))
            {
                await _messagesService.MarkAsReadAsync(m.Id, userId);
            }

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

            return View("Conversation", conversation);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePin(int id)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _messagesService.TogglePinAsync(id, userId);
            return RedirectToAction("Index");
        }
    }
}
