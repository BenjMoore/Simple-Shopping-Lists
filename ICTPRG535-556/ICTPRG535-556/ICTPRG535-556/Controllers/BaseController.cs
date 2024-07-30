using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ICTPRG535_556.Controllers
{
    public class BaseController : Controller
    {
        protected int? LoggedInUserId => HttpContext.Session.GetInt32("UserId");
    }
}
