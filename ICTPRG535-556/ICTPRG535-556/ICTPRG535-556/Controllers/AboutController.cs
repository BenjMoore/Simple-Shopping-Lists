using Microsoft.AspNetCore.Mvc;

namespace ICTPRG535_556.Controllers
{
    public class AboutController : BaseController
    {
        [Route("About")]
        public IActionResult About()
        {
            return View();
        }
    }
}
