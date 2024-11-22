using DataMapper;
using ICTPRG535_556.Controllers;
using ICTPRG535_556.Encrypt;
using ICTPRG535_556.Models;
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
  
    public IActionResult CreateNewList(int userID)
    {
        var loggedInUserId = userID;

        // Check for unsaved lists
        var hasUnsavedList = _dataAccess
            .GetAllUserListsFinalised(loggedInUserId)
            .Any(list => list.FinalisedDate == null);

        // Calculate new list ID
        var maxListId = _dataAccess.GetNewListIdForAll();
        int newListID = maxListId > 0 ? maxListId : 1;

        var newList = new ListDTO
        {
            UserID = loggedInUserId,
            ListID = newListID,
            ItemID = 0,
            ListName = "Cart",
            Quantity = 0
        };

        _dataAccess.AddList(newList);


        HttpContext.Session.SetInt32("ListId", newListID);

        return RedirectToAction("CurrentCart", "Cart");
    }


    [HttpPost]
    [Route("Login")]
    public IActionResult ProcessLogin(string email, string password)
    {
        // Check if email is provided
        if (string.IsNullOrEmpty(email))
        {
            ViewBag.Error = "Email is required";
            return View("Login");
        }
        if (string.IsNullOrEmpty(password))
        {
            ViewBag.Error = "Password is required";
            return View("Login");
        }

        // Get user by email from the database
        var user = _dataAccess.GetUserByEmail(email);

        // Check if user exists
        if (user == null)
        {
            ViewBag.Error = "Invalid email or password.";
            return View("Login");
        }

        // Verify the password using bcrypt
        if (!PasswordUtility.VerifyPassword(password, user.Password))
        {
            ViewBag.Error = "Invalid email or password.";
            return View("Login");
        }

        // Store the user's ID in the session
        HttpContext.Session.SetInt32("UserId", user.UserID);
        bool flag = _dataAccess.HasListsWithoutFinalisedDate(user.UserID);
        if (flag == true) 
        {
            CreateNewList(user.UserID);
        }
        // Redirect to the user's cart page with the userID
        return RedirectToAction("CurrentCart", "Cart");
    }
    [HttpGet]
    [Route("CreateAccount")]
    public IActionResult CreateAccount()
    {
        return View();
    }

    [HttpPost]
    [Route("CreateAccount")]
    public IActionResult ProcessCreateAccount(string Email, string Password, string ConfirmPassword)
    {
        string email = Email.Trim();
        string password = Password.Trim();
        string confirmPassword = ConfirmPassword.Trim();
        // Check if email or password is provided
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            ViewBag.Error = "All fields are required";
            return View("CreateAccount");
        }

        // Check if password and confirm password match
        if (password != confirmPassword)
        {
            ViewBag.Error = "Passwords do not match";
            return View("CreateAccount");
        }

        // Check if the email is already in use
        var existingUser = _dataAccess.GetUserByEmail(email);
        if (existingUser != null)
        {
            ViewBag.Error = "Email is already in use";
            return View("CreateAccount");
        }

        // Hash the password before saving it
        var hashedPassword = PasswordUtility.HashPassword(password);
        
        // Create a new user
        var newUser = new UserDTO
        {
            Email = email,
            Password = hashedPassword,
            Lists = 0
        };

        // Save the new user in the database
        _dataAccess.SetUser(newUser);
        int userid = _dataAccess.GetMaxUserId();
        CreateNewList(userid);
        // Redirect to login page after successful account creation
        ViewBag.Success = "Account created successfully! Please login.";
        return RedirectToAction("Login");
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
