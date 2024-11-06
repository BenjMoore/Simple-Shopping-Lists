using DataMapper;
using ICTPRG535_556.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ICTPRG535_556.Controllers
{

    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DataAccess _dataAccess;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _dataAccess = new DataAccess();
        }

        public IActionResult Index()
        {
            int listId = HttpContext.Session.GetInt32("ListId") ?? 0;
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            var listName = HttpContext.Session.GetString("ListName") ?? "";

            // Pass user email and list name to the view

            ViewData["ListName"] = listName;
            //logic to retrieve user email and selected cart name
            var userEmail = _dataAccess.GetUserEmail(userId); // user email
            var selectedCartName = _dataAccess.GetListNameById(listId); // selected cart name
            ViewData["UserEmail"] = userEmail;
            // logic to retrieve produce items
            var produceItems = _dataAccess.GetProduce();

            // Populate the model
            var model = new SessionCartDTO
            {
                ProduceItems = produceItems,

            };

            // Pass user email to ViewBag
            ViewBag.Email = userEmail;
            ViewBag.LoggedInUserId = userId;

            return View(model);
        }

        public IActionResult GetListName(int listId)
        {
            try
            {
                var listName = _dataAccess.GetListNameById(listId);
                if (listName == null)
                {
                    return NotFound("List not found");
                }
                return Ok(listName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving list name: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // Action to get the user email by user ID
        public IActionResult GetUserEmail(int userId)
        {
            try
            {
                var userEmail = _dataAccess.GetUserEmail(userId);
                if (userEmail == null)
                {
                    return NotFound("User not found");
                }
                return Ok(userEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving user email: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public IActionResult AddToCart(int itemId)
        {
            // Retrieve the selected list ID from session
            var listId = HttpContext.Session.GetInt32("ListId");
            var userId = HttpContext.Session.GetInt32("UserId");

            // Check if the selected list ID or user ID is null or invalid
            if (!listId.HasValue || listId.Value <= 0 || !userId.HasValue || userId.Value <= 0)
            {
                // Handle the case where no list is selected or user is not logged in
                return RedirectToAction("Index", "Home"); // Redirect to a suitable page
            }

            // Retrieve the list name from the database
            var listName = _dataAccess.GetListNameById(listId.Value) ?? "";
            var price = _dataAccess.GetProductPriceByItemId(itemId);

            var listDTO = new ListDTO
            {
                ListID = listId.Value,
                UserID = userId.Value,
                ListName = listName,
                ItemID = itemId,
                Price = price
            };

            // Call the AddList method with the constructed listDTO object


            Boolean flag = _dataAccess.DoesItemExistInCart(Convert.ToInt32(listId), itemId);
            if (flag == false)
            {
                _dataAccess.AddList(listDTO);
                _dataAccess.UpdateItemQuantity(Convert.ToInt32(listId), itemId, 1);
                _dataAccess.UpdateCartPrice(Convert.ToInt32(listId), itemId, 1);
            }
            // Perform quantity validation
            else
            {
                _dataAccess.UpdateItemQuantity(Convert.ToInt32(listId), itemId, 1);
                _dataAccess.UpdateCartPrice(Convert.ToInt32(listId), itemId, 1);
            }

            // Redirect back to the index page
            return RedirectToAction("Index", "Home");
        }




        public IActionResult Search(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return View("Index");
            }

            List<ProduceDTO> searchResults = _dataAccess.GetProduceByItemName(search);
            return PartialView("_ProductCard", searchResults);
        }



        public IActionResult Profile(int userID)
        {
            // Retrieve user information based on the userID
            var user = _dataAccess.GetUserById(userID);
            if (user == null)
            {
                // Redirect to an error page or display an error message
                return View();

            }

            return View(user); // Pass user data to the profile view
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
