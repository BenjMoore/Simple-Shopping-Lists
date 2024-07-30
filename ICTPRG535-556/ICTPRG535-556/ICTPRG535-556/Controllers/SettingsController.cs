using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ICTPRG535_556.Controllers
{
    public class SettingsController : Controller
    {
        [HttpPut]
        [Route("Settings/SetUsername")]
        public IActionResult SetUsername([FromBody] dynamic data)
        {
            string username = data?.username;
            if (!string.IsNullOrEmpty(username))
            {
                // Set the username as a session cookie
                HttpContext.Session.SetString("Username", username);


                TempData["Message"] = "Username has been set.";
                return Json(new { success = true });
            }
            else
            {
                TempData["Error"] = "Username cannot be empty.";
                return Json(new { success = false, error = "Username cannot be empty." });
            }
        }

        [Route("Settings")]
        public IActionResult Settings()
        {
            return View();
        }
    }
}
