using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Messages;
using CarGarage.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarGarage.Controllers
{
    [Authorize]
    public class MessagesController : BaseController
    {
        private readonly IMessagesService _messagesService;
        private readonly IMarketplaceService _marketplaceService;

        public MessagesController(IMessagesService messagesService, IMarketplaceService marketplaceService)
        {
            _messagesService = messagesService;
            _marketplaceService = marketplaceService;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int partId)
        {
            var part = await _marketplaceService.GetByIdAsync(partId);
            if (part == null) return NotFound();

            var model = new MessageFormModel
            {
                PartId = partId,
                ReceiverId = part.OwnerId
            };

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
            }

            return View(msg);
        }

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
