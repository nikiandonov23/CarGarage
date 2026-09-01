using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.Marketplace;
using CarGarage.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarGarage.Controllers
{
    [Authorize]
    public class OffersController : BaseController
    {
        private readonly IOffersService _offersService;

        public OffersController(IOffersService offersService)
        {
            _offersService = offersService;
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
