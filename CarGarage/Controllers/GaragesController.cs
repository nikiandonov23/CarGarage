using CarGarage.Services.Core.Contracts;
using CarGarage.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarGarage.Controllers
{
    [AllowAnonymous]
    public class GaragesController(IGarageService garageService) : BaseController
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var garages = await garageService.GetAllGaragesForMapAsync();
            return View(garages);
        }
    }
}
