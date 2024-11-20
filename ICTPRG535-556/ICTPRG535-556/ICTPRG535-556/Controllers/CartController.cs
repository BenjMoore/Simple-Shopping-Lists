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
    
    public IActionResult CreateNewList()
    {
        var loggedInUserId = HttpContext.Session.GetInt32("UserId")??0;

        // Calculate new list ID
        var maxListId = _dataAccess.GetNewListIdForAll();
        int newListID = maxListId > 0 ? maxListId : 1;
        
        var newList = new ListDTO
        {
            UserID = loggedInUserId,
            ListID = newListID,
            ItemID = 0,
            ListName = "Cart",
            Quantity = 0,
            FinalisedDate = DateTime.Now
        };

        _dataAccess.AddList(newList);
        

        HttpContext.Session.SetInt32("ListId", newListID);

        return RedirectToAction("SelectList", "Cart");
    }
   
    // This loads the select list page where you can select saved lists
    public void SaveLists()
    {
        var loggedInUserId = HttpContext.Session.GetInt32("UserId") ?? 0;

        // Check for unsaved lists
        var hasUnsavedList = _dataAccess
            .GetAllUserListsFinalised(loggedInUserId)
            .Any(list => list.FinalisedDate == null);

        if (hasUnsavedList)
        {
            _dataAccess.FindUnsavedUserLists(loggedInUserId);
        }
    }
    public IActionResult Checkout()
    {
        SaveLists();
        CreateNewList();
        return RedirectToAction("CurrentCart", "Cart");
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

    // this loads the current cart
    public IActionResult CurrentCart()
    {
        if (HttpContext.Session.GetInt32("UserId").HasValue)
        {
            HttpContext.Session.SetInt32("ExistingList",0);

            int loggedInUserId = HttpContext.Session.GetInt32("UserId").Value;

            // Get the most recent list ID for the logged-in user
            var maxListID = _dataAccess.GetCurrentListIdForUser(loggedInUserId);
            HttpContext.Session.SetInt32("ListId", maxListID);

            // Get lists for the logged-in user where FinalisedDate is null (not finalized)
            var userLists = _dataAccess.GetUserListsDTO(loggedInUserId)  // Ensure lists are filtered by UserID
                .Where(list => list.FinalisedDate == null)  // Only consider lists where FinalisedDate is NULL
                .ToList();

            List<SessionCartDTO> cartItems = new List<SessionCartDTO>();
            HashSet<string> addedItems = new HashSet<string>();
            Dictionary<int, decimal> listTotalPrices = new Dictionary<int, decimal>();

            foreach (var list in userLists)
            {
                // Get items for each list
                var listItems = _dataAccess.GetListItems(list.ListID);
                decimal totalPrice = CalculateTotalPrice(listItems); // Calculate the total price of the list
                listTotalPrices[list.ListID] = totalPrice;

                foreach (var listItem in listItems)
                {
                    var userProducts = _dataAccess.GetUserListProducts(listItem.ItemID);

                    foreach (var product in userProducts)
                    {
                        int? nullableQuantity = _dataAccess.GetItemQuantityInList(list.ListID, product.ItemID);
                        int quantity = nullableQuantity ?? 1; // Default to 1 if quantity is null

                        string itemKey = $"{product.ItemID}_{list.ListID}";

                        if (!addedItems.Contains(itemKey))  // Avoid adding duplicate items
                        {
                            var cartItem = new SessionCartDTO
                            {
                                ItemID = product.ItemID,
                                Name = _dataAccess.GetProductName(product.ItemID),
                                Price = Convert.ToString(_dataAccess.GetProductPriceByItemId(product.ItemID)),
                                Unit = _dataAccess.GetProductWeight(product.ItemID),
                                ListID = list.ListID,
                                Quantity = quantity,
                                DateCreated = _dataAccess.GetListCreated(list.ListID)
                            };

                            cartItems.Add(cartItem);
                            addedItems.Add(itemKey);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(list.ListName))
                {
                    HttpContext.Session.SetString("ListName", list.ListName);
                }
            }

            // Calculate the grand total of all lists
            decimal grandTotal = listTotalPrices.Values.Sum();

            // Pass the grand total and list prices to the view
            ViewBag.ListTotalPrices = listTotalPrices;
            ViewBag.GrandTotal = grandTotal;

            return View(cartItems); // Pass the cart items to the view
        }
        else
        {
            return RedirectToAction("Login", "Auth"); // Redirect to login if not logged in
        }
    }
    
    public IActionResult SelectList(int listId)
    {
        // Check if the user is logged in
        if (HttpContext.Session.GetInt32("UserId").HasValue)
        {
            // Save the selected list ID in the session
            HttpContext.Session.SetInt32("ListId", listId);
            HttpContext.Session.SetInt32("ExistingList", 1);
            var id = HttpContext.Session.GetInt32("UserId");

            var list = _dataAccess.GetListById((int)id);

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
                        Quantity = quantity,
                        DateCreated = _dataAccess.GetListCreated(listId)

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
    public IActionResult DeleteProduce(int itemId)
    {
        var userID = HttpContext.Session.GetInt32("UserId") ?? 0;
        var listID = HttpContext.Session.GetInt32("ListId") ?? 0;

        try
        {
            // Call the DeleteProduce method in your data access layer
            _dataAccess.DeleteProduce(listID, itemId, userID);

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


