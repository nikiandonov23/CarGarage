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
    }
}
