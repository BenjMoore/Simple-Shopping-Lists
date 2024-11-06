using DataMapper;
using ICTPRG535_556.Controllers;
using ICTPRG535_556.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class CartController : BaseController
{
    private readonly DataAccess _dataAccess;

    public CartController()
    {
        _dataAccess = new DataAccess();
    }

    public int selectedList = 0;

    // Calculates total list price
    private decimal CalculateTotalPrice(List<ListDTO> items)
    {
        decimal totalPrice = 0;
        foreach (var item in items)
        {
            totalPrice += item.Price;
        }
        return totalPrice;
    }



    [Route("/Cart")]
    public IActionResult UserCart(int id)
    {
        if (LoggedInUserId.HasValue)
        {
            // Retrieve list IDs for the user
            var listIDs = _dataAccess.GetUserLists(id);

            // Get the first list ID
            int? firstListId = listIDs.FirstOrDefault();

            if (firstListId != null)
            {
                // Retrieve list items for the first list
                List<ListDTO> listItems = _dataAccess.GetListItems(firstListId.Value);
                List<ProduceDTO> cartItems = new List<ProduceDTO>();

                // Iterate through each list item
                foreach (var listItem in listItems)
                {
                    
                    // Retrieve products for the current list item
                    var userProducts = _dataAccess.GetUserListProducts(listItem.ItemID);

                    // Iterate through the products and fetch details for each item
                    foreach (var product in userProducts)
                    {
                        // Create a new cart item and add it to the list
                        var cartItem = new ProduceDTO
                        {
                            ItemID = product.ItemID,
                            Name = _dataAccess.GetProductName(product.ItemID),
                            Price = _dataAccess.GetProductPriceByItemId(product.ItemID),
                            Unit = _dataAccess.GetProductWeight(product.ItemID)
                        };

                        cartItems.Add(cartItem);
                    }
                }

                // Pass cart items to the view
                return View(cartItems);
            }
            else
            {
                // Handle the case where the user has no lists
                return View("NoLists");
            }
        }
        else
        {
            // User is not logged in, redirect to login page or handle as needed
            return RedirectToAction("Login", "Auth");
        }
    }
    // Creates a new list
    public IActionResult CreateNewList(int listID)
    {
        var loggedInUserId = HttpContext.Session.GetInt32("UserId") ?? 0;
        var maxlistid = _dataAccess.GetMaxListIdForUser(Convert.ToInt32(loggedInUserId));
        int newListID = Convert.ToInt32(maxlistid) + 1;
        var newList = new ListDTO
        {
            UserID = loggedInUserId,
            ListID = newListID,
            ItemID = 0
        };
        _dataAccess.AddList(newList);
        HttpContext.Session.SetInt32("ListId", newListID);
        return RedirectToAction("SelectList", "Cart");
    }
    public IActionResult Select()
    {
        // Check if the logged-in user ID is available in the session
        if (HttpContext.Session.GetInt32("UserId").HasValue)
        {
            // Retrieve the logged-in user ID from the session
            int loggedInUserId = HttpContext.Session.GetInt32("UserId").Value;

            // Retrieve all lists for the logged-in user and filter for finalized lists
            var userLists = _dataAccess.GetAllUserListsFinalised(loggedInUserId)
                                       .Where(list => list.FinalisedDate != null)
                                       .ToList();

            // Dictionary to hold list IDs and their total prices
            Dictionary<int, decimal> listTotalPrices = new Dictionary<int, decimal>();

            // Calculate total price for each finalized list
            foreach (var list in userLists)
            {
                var listItems = _dataAccess.GetListItems(list.ListID);
                decimal totalPrice = CalculateTotalPrice(listItems);
                listTotalPrices[list.ListID] = totalPrice;
            }

            // Pass list total prices to the view using ViewBag
            ViewBag.ListTotalPrices = listTotalPrices;

            return View(userLists);
        }
        else
        {
            return RedirectToAction("Login", "Auth");
        }
    }
    public IActionResult CurrentList()
    {
        if (HttpContext.Session.GetInt32("UserId").HasValue)
        {
            int loggedInUserId = HttpContext.Session.GetInt32("UserId").Value;

            // Retrieve the first non-finalized list for the user
            var userLists = _dataAccess.GetAllUserLists(loggedInUserId);
            var currentCart = userLists.FirstOrDefault(list => list.FinalisedDate == null);

            if (currentCart != null)
            {
                // Get list items for the current cart
                var listItems = _dataAccess.GetListItems(currentCart.ListID);
                decimal totalPrice = CalculateTotalPrice(listItems);

                // Use SessionCartDTO to encapsulate list and produce item details
                var sessionCart = new SessionCartDTO
                {
                    ListID = currentCart.ListID,
                    ListName = currentCart.ListName,
                    UserID = loggedInUserId,
                    FinalisedDate = currentCart.FinalisedDate,
                    Quantity = listItems.Sum(item => item.Quantity),
                    ProduceItems = listItems.Select(item => new ProduceDTO
                    {
                        ItemID = item.ItemID,
                        Name = item.ListName,
                        Unit = item.Unit,
                        Price = item.Price,
                        Quantity = item.Quantity // Assign Quantity here if available
                    }).ToList()

                };

                ViewData["TotalPrice"] = totalPrice;
                return PartialView("CurrentCart", sessionCart);
            }
            else
            {
                ViewBag.Message = "No current cart found.";
                return PartialView("CurrentCart", null);
            }
        }
        else
        {
            return RedirectToAction("Login", "Auth");
        }
    }



    public IActionResult SelectList(int listId)
    {
        // Check if the user is logged in
        if (HttpContext.Session.GetInt32("UserId").HasValue)
        {
            // Save the selected list ID in the session
            HttpContext.Session.SetInt32("ListId", listId);

            var list = _dataAccess.GetListById(listId);
            if (list == null)
            {
                // Handle the case where the list is not found
                return RedirectToAction("UserCart", "Cart");
            }

            List<ListDTO> listItems = _dataAccess.GetListItems(listId);
            List<SessionCartDTO> cartItems = new List<SessionCartDTO>();

            // Get produce information for each item in the list
            foreach (var listItem in listItems)
            {
                // Retrieve products for the current list item
                var userProducts = _dataAccess.GetUserListProducts(listItem.ItemID);

                // Iterate through the products and fetch details for each item
                foreach (var product in userProducts)
                {
                    int? nullableQuantity = _dataAccess.GetItemQuantityInList(listId, product.ItemID);
                    int quantity = nullableQuantity ?? 1;

                    // Create a new cart item and add it to the list
                    var cartItem = new SessionCartDTO
                    {
                        ItemID = product.ItemID,
                        Name = _dataAccess.GetProductName(product.ItemID),
                        Price = Convert.ToString(_dataAccess.GetProductPriceByItemId(product.ItemID)),
                        Unit = _dataAccess.GetProductWeight(product.ItemID),
                        ListID = listId,
                        Quantity = quantity

                    };

                    cartItems.Add(cartItem);
                }
                // Null check before setting ListName in session
                if (!string.IsNullOrEmpty(listItem.ListName))
                {
                    HttpContext.Session.SetString("ListName", listItem.ListName);
                }
            }

            // Calculate total price for the current list
            decimal totalPrice = CalculateTotalPrice(listItems);

            // Pass cart items and total price to the view
            ViewBag.TotalPrice = totalPrice;
            return View(cartItems);
        }
        else
        {
            // User is not logged in, redirect to the login page
            return RedirectToAction("Login", "Auth");
        }
    }



    [HttpPost]
    public IActionResult UpdateListName(int listId, string newListName)
    {
        try
        {
            // Call DataAccess layer method to update list name
            _dataAccess.UpdateListName(listId, newListName);
            // Return success response as JSON
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            // Return 500 status with the exception message if an error occurs
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    public IActionResult UpdateQuantity(int itemId, int listId, int quantityChange)
    {
        // Retrieve userId from session
        int userId = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
        try
        {
            // Retrieve current state of the shopping list
            List<ListDTO> currentListState = _dataAccess.GetListItems(listId);
            // Retrieve current quantity of the item in the list
            int currentQuantity = _dataAccess.GetItemQuantityInList(listId, itemId);
            // Calculate new quantity after change
            int newQuantity = currentQuantity + quantityChange;

            // If new quantity is zero, delete the item from the list
            if (newQuantity == 0)
            {
                _dataAccess.DeleteProduce(listId, itemId, userId);
                // Return success response with updated quantities and total price
                return Json(new { success = true, newQuantity = 0, newPrice = 0, totalPrice = CalculateTotalPrice(currentListState) });
            }
            else
            {
                // Update the quantity of the item in the list
                _dataAccess.UpdateItemQuantity(listId, itemId, quantityChange);
                // Update calculated price for the item in the cart
                _dataAccess.UpdateCartPrice(listId, itemId, newQuantity);

                // Calculate new price for the item based on new quantity
                decimal newPrice = _dataAccess.GetProductPriceByItemId(itemId) * newQuantity;

                // Retrieve updated state of the shopping list
                List<ListDTO> newListState = _dataAccess.GetListItems(listId);

                // Calculate total price of the shopping list
                decimal totalPrice = CalculateTotalPrice(newListState);

                // Return success response with updated quantities, prices, and total price
                return Json(new { success = true, newQuantity = newQuantity, newPrice = newPrice.ToString("F2"), totalPrice = totalPrice.ToString("F2") });
            }
        }
        catch (Exception ex)
        {
            // Return 500 status 
            return StatusCode(500, ex.Message);
        }
    }




    [HttpPost]
    [Route("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
    [HttpPost]
    public IActionResult DeleteList(int listId)
    {
        // Add logic to delete the list with the specified ID
        _dataAccess.DeleteList(listId);
        return RedirectToAction(nameof(SelectList), new { listId }); // Redirect to the same action with the listId
    }

    [HttpPost]
    public IActionResult DeleteProduce(int itemId, int userId)
    {
        int listId = 0;
        HttpContext.Session.SetInt32("ListId", listId);
        try
        {
            // Call the DeleteProduce method in your data access layer
            _dataAccess.DeleteProduce(itemId, userId, listId);

            // Return a success response 
            return Ok();
        }
        catch (Exception ex)
        {
            // Return an error response 
            return StatusCode(500, ex.Message);
        }
    }



}


