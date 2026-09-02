
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

            // Ако вече сме в разговор,
            // използваме съществуващия ConversationId.
            //
            // Ако това е нов разговор,
            // създаваме нов уникален ConversationId.
            var conversationKey =
                model.ConversationId;

            if (string.IsNullOrEmpty(conversationKey))
            {
                conversationKey =
                    Guid.NewGuid().ToString();
            }

            // Създаваме съобщението
            var sentMessage =
                await _messagesService.AddMessageAsync(
                    senderId,
                    model.ReceiverId,
                    model.Content,
                    conversationKey);

            // Взимаме името на сервиза на изпращача
            var senderGarage =
                await _garageService
                    .GetGarageDetailsAsync(senderId);

            var senderGarageName =
                senderGarage?.Name
                ?? "Сервиз";

            // Изпращаме новото съобщение в реално време
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
                            conversationId =
                                sentMessage.ConversationId,

                            senderGarageName =
                                senderGarageName
                        });

                // Обновяваме unread badge
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
                // SignalR проблемът не трябва
                // да проваля записването на съобщението.
            }


            // Ако сме в съществуващ разговор,
            // оставаме в него.
            //
            // Вече имаме ConversationId,
            // затова използваме него.
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

            // Вече получаваме както изпратените,
            // така и получените съобщения.
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

                        LatestMessageId =
                            latest.Id,

                        OtherUserId =
                            otherUserId,

                        LastMessage =
                            latest.Content,

                        LastSentAt =
                            latest.SentAt,

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


            // Взимаме името на сервиза
            // вместо Username.
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
                    conv.OtherUserName =
                        garage.Name;
                }
                else
                {
                    // Fallback към Username
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

            // Намираме другия участник
            var otherUserId =
                messages
                    .FirstOrDefault(
                        m => m.SenderId != userId)
                    ?.SenderId
                ?? messages
                    .FirstOrDefault()
                    ?.ReceiverId;

            // Взимаме името на неговия сервиз
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
                    otherGarageName =
                        garage.Name;
                }
                else
                {
                    // Fallback към Username
                    var usr =
                        await _userManager
                            .FindByIdAsync(
                                otherUserId);

                    otherGarageName =
                        usr?.UserName ??
                        otherUserId;
                }
            }

            // Подаваме името към Conversation.cshtml
            ViewBag.OtherGarageName =
                otherGarageName;


            // Маркираме получените съобщения
            // като прочетени.
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(
            int id)
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
    }
}

