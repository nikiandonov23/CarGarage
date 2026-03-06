using Microsoft.AspNetCore.Mvc;

namespace CarGarage.Web.Controllers
{
    
    public class MyGarageController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
