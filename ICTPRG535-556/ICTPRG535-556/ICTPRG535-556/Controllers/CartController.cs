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

    // Calculates the total for a list
    private decimal CalculateTotalPrice(List<ListDTO> items)
    {
        decimal totalPrice = 0;
        foreach (var item in items)
        {
            totalPrice += item.Price;
        }
        return totalPrice;
    }


    // User Cart Section for selected cart
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

    // Creates a new list for user
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

    // Select cart page to display user carts
    public IActionResult Select()
    {
        // Check if the logged-in user ID is available in the session
        if (HttpContext.Session.GetInt32("UserId").HasValue)
        {
            // Retrieve the logged-in user ID from the session
            int loggedInUserId = HttpContext.Session.GetInt32("UserId").Value;

            // Retrieve all lists for the logged-in user
            var userLists = _dataAccess.GetAllUserLists(loggedInUserId);

            // Dictionary to hold list IDs and their total prices
            Dictionary<int, decimal> listTotalPrices = new Dictionary<int, decimal>();

            // Calculate total price for each list
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
                return RedirectToAction("UserCart", "Cart"); // Redirect to a suitable page
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



    // Updates List Name based on listid and listname
    [HttpPost]
    public IActionResult UpdateListName(int listId, string newListName)
    {
        try
        {
            _dataAccess.UpdateListName(listId, newListName);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    // Updates quantity for duplicate products in the list
    [HttpPost]
    public IActionResult UpdateQuantity(int itemId, int listId, int quantityChange)
    {
        int userId = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
        try
        {
            List<ListDTO> currentListState = _dataAccess.GetListItems(listId);
            int currentQuantity = _dataAccess.GetItemQuantityInList(listId, itemId);
            int newQuantity = currentQuantity + quantityChange;

            if (newQuantity == 0)
            {
                _dataAccess.DeleteProduce(listId, itemId, userId);
                return Json(new { success = true, newQuantity = 0, newPrice = 0, totalPrice = CalculateTotalPrice(currentListState) });
            }
            else
            {
                _dataAccess.UpdateItemQuantity(listId, itemId, quantityChange);

                _dataAccess.UpdateCartPrice(listId, itemId, newQuantity);

                decimal newPrice = _dataAccess.GetProductPriceByItemId(itemId) * newQuantity;

                List<ListDTO> newListState = _dataAccess.GetListItems(listId);

                decimal totalPrice = CalculateTotalPrice(newListState);

                return Json(new { success = true, newQuantity = newQuantity, newPrice = newPrice.ToString("F2"), totalPrice = totalPrice.ToString("F2") });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }




    // Logout Method
    [HttpPost]
    [Route("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
    // Delete List
    [HttpPost]
    public IActionResult DeleteList(int listId)
    {
        _dataAccess.DeleteList(listId);
        return RedirectToAction(nameof(SelectList), new { listId }); // Redirect to the same action with the listId
    }
    // Delete Produce
    [HttpPost]
    public IActionResult DeleteProduce(int itemId, int userId)
    {
        int listId = 0;
        HttpContext.Session.SetInt32("ListId", listId);
        try
        {
            // Call the DeleteProduce method in your data access layer
            _dataAccess.DeleteProduce(itemId, userId, listId);

            // Return a success response for AJAX
            return Ok();
        }
        catch (Exception ex)
        {
            // Return an error response for AJAX
            return StatusCode(500, ex.Message);
        }
    }



}


