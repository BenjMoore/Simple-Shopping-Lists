using DataMapper;
using ICTPRG535_556.Controllers;
using Microsoft.AspNetCore.Mvc;

public class AuthController : BaseController
{
    private readonly DataAccess _dataAccess;

    public AuthController()
    {
        _dataAccess = new DataAccess();
    }

    [HttpGet]
    [Route("Login")]
    public IActionResult Login()
    {
        if (LoggedInUserId.HasValue)
        {
            var user = _dataAccess.GetUserById(LoggedInUserId.Value);
            ViewBag.Email = user.Email; 
            return View("LoggedIn");
        }
        return View();
    }

    [HttpPost]
    [Route("Login")]
    public IActionResult ProcessLogin(string email)
    {
        // Check if email is provided
        if (string.IsNullOrEmpty(email))
        {
            ViewBag.Error = "Email is required";
            return View("Login");
        }

        // Get user by email from the database
        var user = _dataAccess.GetUserByEmail(email);

        // Check if user exists
        if (user == null)
        {
            ViewBag.Error = "Invalid email";
            return View("Login");
        }

        // Store the user's ID in the session
        HttpContext.Session.SetInt32("UserId", user.UserID);

        // Redirect to the user's cart page with the userID
        return RedirectToAction("UserCart", "Cart", new { id = user.UserID });
    }
    public int GetUserId()
    {
        if (HttpContext == null)
        {
            throw new InvalidOperationException("UserId is Null!");
        }

        int? userId = HttpContext.Session.GetInt32("UserId");

        if (!userId.HasValue)
        {
            throw new InvalidOperationException("UserId is not available in session.");
        }
        return Convert.ToInt32(userId);
    }

    public IActionResult Logout()
    {
        // Clear the session data
        HttpContext.Session.Clear();

        // Invalidate the authentication cookie
        Response.Cookies.Delete(".AspNetCore.Cookies");

        // Redirect to the login page or the home page
        return RedirectToAction("Index", "Home");
    }

}
