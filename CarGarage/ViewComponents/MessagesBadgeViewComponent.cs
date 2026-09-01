using System.Security.Claims;
using CarGarage.Services.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CarGarage.ViewComponents
{
    public class MessagesBadgeViewComponent : ViewComponent
    {
        private readonly IMessagesService _messagesService;

        public MessagesBadgeViewComponent(IMessagesService messagesService)
        {
            _messagesService = messagesService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsUser = User as ClaimsPrincipal ?? HttpContext?.User as ClaimsPrincipal;
            var userId = claimsUser?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return View(0);
            }

            var count = await _messagesService.GetUnreadCountAsync(userId);
            return View(count);
        }
    }
}
