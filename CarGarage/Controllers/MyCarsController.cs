using CarGarage.Services.Core.Contracts;
using CarGarage.ViewModels.MyCars;
using CarGarage.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class MyCarsController(IMyCarsService carsService) : BaseController
{
    // Index
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var model = await carsService.GetAllUserCarsAsync(userId);
        return View(model);
    }
    
    // GET: Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = await Task.FromResult(new CreateCarViewModel());
        return View(model);
    }

    

  
}
