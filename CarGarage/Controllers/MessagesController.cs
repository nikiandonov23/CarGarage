using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Messages;
using CarGarage.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;

namespace CarGarage.Controllers
{
    [Authorize]
    public class MessagesController : BaseController
    {
        private readonly IMessagesService _messagesService;
        private readonly IMarketplaceService _marketplaceService;
        private readonly IHubContext<Notifications.NotificationsHub> _hubContext;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IGarageService _garageService;

        public MessagesController(
            IMessagesService messagesService,
            IMarketplaceService marketplaceService,
            IHubContext<Notifications.NotificationsHub> hubContext,
            UserManager<IdentityUser> userManager,
            IGarageService garageService)
        {
            _messagesService = messagesService;
            _marketplaceService = marketplaceService;
            _hubContext = hubContext;
            _userManager = userManager;
            _garageService = garageService;
        }

        [HttpGet]
        public async Task<IActionResult> Create(
            int? partId,
            string? receiverId,
            string? conversationId)
        {
            MessageFormModel model;

            if (!string.IsNullOrEmpty(receiverId))
            {
                model = new MessageFormModel
                {
                    PartId = partId,
                    ReceiverId = receiverId,
                    ConversationId = conversationId
                };
            }
            else if (partId.HasValue)
            {
                var part = await _marketplaceService
                    .GetByIdAsync(partId.Value);

                if (part == null)
                    return NotFound();

                model = new MessageFormModel
                {
                    PartId = partId,
                    ReceiverId = part.OwnerId,
                    ConversationId = conversationId
                };
            }
            else
            {
                return BadRequest();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MessageFormModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var senderId = GetUserId();

            if (string.IsNullOrEmpty(senderId))
                return Unauthorized();

            var conversationKey = model.ConversationId;

            if (string.IsNullOrEmpty(conversationKey))
            {
                conversationKey = Guid.NewGuid().ToString();
            }

            var sentMessage =
                await _messagesService.AddMessageAsync(
                    senderId,
                    model.ReceiverId,
                    model.Content,
                    conversationKey);

            var senderGarage =
                await _garageService
                    .GetGarageDetailsAsync(senderId);

            var senderGarageName =
                senderGarage?.Name
                ?? "Сервиз";

            try
            {
                await _hubContext.Clients
                    .User(model.ReceiverId)
                    .SendAsync(
                        "NewMessage",
                        new
                        {
                            id = sentMessage.Id,
                            senderId = sentMessage.SenderId,
                            receiverId = sentMessage.ReceiverId,
                            content = sentMessage.Content,
                            sentAt = sentMessage.SentAt,
                            conversationId = sentMessage.ConversationId,
                            senderGarageName = senderGarageName
                        });

                var unreadCount =
                    await _messagesService
                        .GetUnreadCountAsync(
                            model.ReceiverId);

                await _hubContext.Clients
                    .User(model.ReceiverId)
                    .SendAsync(
                        "UnreadCountUpdated",
                        unreadCount);
            }
            catch
            {
                // SignalR error suppression
            }

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = sentMessage.Id
                });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var inbox =
                (await _messagesService
                    .GetInboxAsync(userId))
                .ToList();

            var groups = inbox
                .GroupBy(m =>
                    !string.IsNullOrEmpty(m.ConversationId)
                        ? m.ConversationId
                        : string.Compare(
                            m.SenderId,
                            m.ReceiverId) < 0
                            ? $"{m.SenderId}_{m.ReceiverId}"
                            : $"{m.ReceiverId}_{m.SenderId}")
                .Select(g =>
                {
                    var latest =
                        g.OrderByDescending(x => x.SentAt)
                         .First();

                    var otherUserId =
                        g.SelectMany(x =>
                                new[]
                                {
                                    x.SenderId,
                                    x.ReceiverId
                                })
                         .Where(x => x != userId)
                         .Distinct()
                         .First();

                    return new ConversationSummaryViewModel
                    {
                        ConversationId = g.Key,
                        LatestMessageId = latest.Id,
                        OtherUserId = otherUserId,
                        LastMessage = latest.Content,
                        LastSentAt = latest.SentAt,
                        UnreadCount =
                            g.Count(x =>
                                !x.IsRead &&
                                x.ReceiverId == userId),
                        IsPinned =
                            g.Any(x => x.IsPinned)
                    };
                })
                .OrderByDescending(g => g.LastSentAt)
                .ToList();

            foreach (var conv in groups)
            {
                var garage =
                    await _garageService
                        .GetGarageDetailsAsync(
                            conv.OtherUserId);

                if (garage != null &&
                    !string.IsNullOrWhiteSpace(
                        garage.Name))
                {
                    conv.OtherUserName = garage.Name;
                }
                else
                {
                    var usr =
                        await _userManager
                            .FindByIdAsync(
                                conv.OtherUserId);

                    conv.OtherUserName =
                        usr?.UserName ??
                        conv.OtherUserId;
                }
            }

            return View(groups);
        }

        [HttpGet]
        public async Task<IActionResult> Outbox()
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var outbox =
                await _messagesService
                    .GetOutboxAsync(userId);

            return View(outbox);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var conversation =
                await _messagesService
                    .GetConversationAsync(
                        id,
                        userId);

            if (conversation == null ||
                !conversation.Any())
            {
                return NotFound();
            }

            var messages =
                conversation
                    .OrderBy(m => m.SentAt)
                    .ToList();

            var otherUserId =
                messages
                    .FirstOrDefault(
                        m => m.SenderId != userId)
                    ?.SenderId
                ?? messages
                    .FirstOrDefault()
                    ?.ReceiverId;

            string? otherGarageName = null;

            if (!string.IsNullOrEmpty(otherUserId))
            {
                var garage =
                    await _garageService
                        .GetGarageDetailsAsync(
                            otherUserId);

                if (garage != null &&
                    !string.IsNullOrWhiteSpace(
                        garage.Name))
                {
                    otherGarageName = garage.Name;
                }
                else
                {
                    var usr =
                        await _userManager
                            .FindByIdAsync(
                                otherUserId);

                    otherGarageName =
                        usr?.UserName ??
                        otherUserId;
                }
            }

            ViewBag.OtherGarageName = otherGarageName;

            foreach (var message in conversation
                .Where(x =>
                    x.ReceiverId == userId &&
                    !x.IsRead))
            {
                await _messagesService
                    .MarkAsReadAsync(
                        message.Id,
                        userId);
            }

            try
            {
                var unread =
                    await _messagesService
                        .GetUnreadCountAsync(
                            userId);

                await _hubContext.Clients
                    .User(userId)
                    .SendAsync(
                        "UnreadCountUpdated",
                        unread);
            }
            catch
            {
                // Ignore SignalR errors
            }

            return View(
                "Conversation",
                conversation);
        }

        // --- ДОБАВЕН GET МЕТОД ЗА ИЗТРИВАНЕ (ЗАРЕЖДА ВЮТО) ---
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            // Можете да заредите съобщението или детайлите, за да ги визуализирате във вюто
            var conversation = await _messagesService.GetConversationAsync(id, userId);
            var message = conversation?.OrderByDescending(m => m.SentAt).FirstOrDefault();

            if (message == null)
                return NotFound();

            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, IFormCollection collection)
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _messagesService
                .DeleteAsync(
                    id,
                    userId);

            return RedirectToAction(
                nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePin(
            int id)
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _messagesService
                .TogglePinAsync(
                    id,
                    userId);

            return RedirectToAction(
                nameof(Index));
        }

        /// <summary>
        /// API endpoint to get current user ID
        /// </summary>
        [HttpGet]
        [Route("api/messages/current-user")]
        public IActionResult GetCurrentUser()
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            return Ok(new { userId });
        }

        /// <summary>
        /// API endpoint for floating chat widget - returns unread conversations grouped by sender with full conversation history
        /// </summary>
        [HttpGet]
        [Route("api/messages/unread-conversations")]
        public async Task<IActionResult> GetUnreadConversations()
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var unreadMessages = await _messagesService
                .GetUnreadConversationsAsync(userId);

            // Group unread messages to identify conversations
            var conversationGroups = unreadMessages
                .GroupBy(m => new { m.SenderId, m.ConversationId })
                .Select(g => new
                {
                    senderId = g.Key.SenderId,
                    conversationId = g.Key.ConversationId ?? $"{g.Key.SenderId}_{userId}",
                    latestMessage = g.OrderByDescending(x => x.SentAt).FirstOrDefault()?.Content,
                    latestMessageTime = g.OrderByDescending(x => x.SentAt).FirstOrDefault()?.SentAt,
                    unreadCount = g.Count()
                })
                .OrderByDescending(x => x.latestMessageTime)
                .ToList();

            // Get sender info and full conversation history for each conversation
            var result = new List<dynamic>();
            foreach (var conv in conversationGroups)
            {
                var garage = await _garageService
                    .GetGarageDetailsAsync(conv.senderId);

                var senderName = garage?.Name ?? 
                    (await _userManager.FindByIdAsync(conv.senderId))?.UserName ?? 
                    conv.senderId;

                // Load full conversation history (not just unread messages)
                var fullConversation = await _messagesService
                    .GetConversationByIdAsync(conv.conversationId, userId);

                var messages = fullConversation
                    .OrderBy(x => x.SentAt)
                    .Select(m => new
                    {
                        id = m.Id,
                        content = m.Content,
                        sentAt = m.SentAt,
                        isRead = m.IsRead,
                        senderId = m.SenderId
                    }).ToList();

                result.Add(new
                {
                    conv.senderId,
                    conv.conversationId,
                    senderName,
                    conv.latestMessage,
                    conv.latestMessageTime,
                    conv.unreadCount,
                    messages
                });
            }

            return Ok(result);
        }

        /// <summary>
        /// API endpoint to mark all messages in a conversation as read
        /// </summary>
        [HttpPost]
        [Route("api/messages/mark-conversation-read")]
        public async Task<IActionResult> MarkConversationAsRead(
            [FromBody] MarkConversationReadRequest request)
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (string.IsNullOrEmpty(request?.ConversationId))
                return BadRequest("ConversationId is required");

            await _messagesService
                .MarkConversationAsReadAsync(
                    request.ConversationId,
                    userId);

            // Update unread count and notify via SignalR
            var unreadCount = await _messagesService
                .GetUnreadCountAsync(userId);

            try
            {
                await _hubContext.Clients
                    .User(userId)
                    .SendAsync("UnreadCountUpdated", unreadCount);
            }
            catch
            {
                // SignalR error suppression
            }

            return Ok(new { unreadCount });
        }

        /// <summary>
        /// API endpoint to send a message from floating chat widget
        /// </summary>
        [HttpPost]
        [Route("api/messages/send-from-widget")]
        public async Task<IActionResult> SendMessageFromWidget(
            [FromBody] SendMessageWidgetRequest request)
        {
            var senderId = GetUserId();

            if (string.IsNullOrEmpty(senderId))
                return Unauthorized();

            if (string.IsNullOrEmpty(request?.ReceiverId) || 
                string.IsNullOrEmpty(request?.Content))
                return BadRequest("ReceiverId and Content are required");

            var sentMessage = await _messagesService
                .AddMessageAsync(
                    senderId,
                    request.ReceiverId,
                    request.Content,
                    request.ConversationId);

            var senderGarage = await _garageService
                .GetGarageDetailsAsync(senderId);

            var senderName = senderGarage?.Name ?? 
                (await _userManager.FindByIdAsync(senderId))?.UserName ?? 
                senderId;

            try
            {
                // Notify receiver about new message
                await _hubContext.Clients
                    .User(request.ReceiverId)
                    .SendAsync("NewMessage", new
                    {
                        id = sentMessage.Id,
                        senderId = sentMessage.SenderId,
                        receiverId = sentMessage.ReceiverId,
                        content = sentMessage.Content,
                        sentAt = sentMessage.SentAt,
                        conversationId = sentMessage.ConversationId,
                        senderName
                    });

                // Update receiver's unread count
                var receiverUnreadCount = await _messagesService
                    .GetUnreadCountAsync(request.ReceiverId);

                await _hubContext.Clients
                    .User(request.ReceiverId)
                    .SendAsync("UnreadCountUpdated", receiverUnreadCount);
            }
            catch
            {
                // SignalR error suppression
            }

            return Ok(new 
            { 
                id = sentMessage.Id,
                conversationId = sentMessage.ConversationId,
                sentAt = sentMessage.SentAt
            });
        }
    }

    /// <summary>
    /// Request model for marking conversation as read
    /// </summary>
    public class MarkConversationReadRequest
    {
        public string? ConversationId { get; set; }
    }

    /// <summary>
    /// Request model for sending message from widget
    /// </summary>
    public class SendMessageWidgetRequest
    {
        public string? ReceiverId { get; set; }
        public string? Content { get; set; }
        public string? ConversationId { get; set; }
    }
}