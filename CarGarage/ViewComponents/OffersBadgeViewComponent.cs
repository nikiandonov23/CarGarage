using System.Security.Claims;
using CarGarage.Services.Core.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace CarGarage.ViewComponents
{
    public class OffersBadgeViewComponent : ViewComponent
    {
        private readonly IOffersService _offersService;

        public OffersBadgeViewComponent(IOffersService offersService)
        {
            _offersService = offersService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsUser = User as ClaimsPrincipal ?? HttpContext?.User as ClaimsPrincipal;
            var userId = claimsUser?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return View(0);

            var offers = await _offersService.GetPendingOffersForOwnerAsync(userId);
            var count = offers?.Count() ?? 0;
            return View(count);
        }
    }
}
