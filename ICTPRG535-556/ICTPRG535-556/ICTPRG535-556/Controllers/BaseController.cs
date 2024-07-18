using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ICTPRG535_556.Controllers
{
    public class BaseController : Controller
    {
        // Define a property to access the logged-in user ID stored in the session
        protected int? LoggedInUserId => HttpContext.Session.GetInt32("UserId");
    }
}
