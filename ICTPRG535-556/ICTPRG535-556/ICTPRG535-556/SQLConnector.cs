using ICTPRG535_556.Models;
using Dapper;
using System.Collections;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using NuGet.Protocol.Plugins;
using ICTPRG535_556.Encrypt;
namespace DataMapper
{
    public class DataAccess
    {
        private readonly string connectionString;
        private readonly string serverCon;

        public DataAccess()
        {
            this.connectionString = "Server=localhost;Database=ShoppingList;Trusted_Connection=True;TrustServerCertificate=True;";
            this.serverCon = "Server=localhost;Trusted_Connection=True;TrustServerCertificate=True;";


        }

    #region Initialise Database


        public void SetupDatabaseAndTables()
        {
            using (SqlConnection connection = new SqlConnection(serverCon))
            {
                connection.Open();

                try
                {
                    // Check if the database exists, and create it if it does not.
                    string createDatabaseQuery = @"
            IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'ShoppingList')
            BEGIN
                CREATE DATABASE ShoppingList;
            END;";
                    SqlCommand createDatabaseCommand = new SqlCommand(createDatabaseQuery, connection);
                    createDatabaseCommand.ExecuteNonQuery();

                    // Switch to the ShoppingList database.
                    string useDatabaseQuery = "USE ShoppingList;";
                    SqlCommand useDatabaseCommand = new SqlCommand(useDatabaseQuery, connection);
                    useDatabaseCommand.ExecuteNonQuery();

                    // Create the Lists table if it does not exist.
                    string createTableListsQuery = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Lists')
            BEGIN
                CREATE TABLE Lists (
                    ListID INT NOT NULL,
                    UserID INT NOT NULL,
                    ItemID INT NOT NULL,
                    ListName NCHAR(50),
                    Quantity INT,
                    Price FLOAT,
                    FinalisedDate DATETIME,
                    Date DATETIME 
                );
            END;";
                    SqlCommand createTableListsCommand = new SqlCommand(createTableListsQuery, connection);
                    createTableListsCommand.ExecuteNonQuery();

                    // Create the Users table if it does not exist.
                    string createTableUsersQuery = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Users')
                    BEGIN
                        CREATE TABLE Users (
                            UserID INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
                            Email NCHAR(50) NOT NULL,
                            Password VARCHAR(200) NOT NULL,
                            Lists INT NOT NULL
                        );
                    END;
                    ";
                    SqlCommand createTableUsersCommand = new SqlCommand(createTableUsersQuery, connection);
                    createTableUsersCommand.ExecuteNonQuery();

                    // Create the Produce table if it does not exist.
                    string createTableProduceQuery = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Produce')
            BEGIN
                CREATE TABLE Produce (
                    ItemID INT NOT NULL PRIMARY KEY,
                    Name NCHAR(60) NOT NULL,
                    Unit NCHAR(30) NOT NULL,
                    Price VARCHAR(30) NOT NULL
                );
            END;";
                    SqlCommand createTableProduceCommand = new SqlCommand(createTableProduceQuery, connection);
                    createTableProduceCommand.ExecuteNonQuery();
                    // Populate the tables if they are empty.
                    string plainPassword = "Test123$";
                    string hashedPassword = PasswordUtility.HashPassword(plainPassword);
                    string checkAndPopulateTablesQuery = $@"
            USE ShoppingList;

            -- Check if the Lists table is empty
            IF NOT EXISTS (SELECT 1 FROM Lists)
            BEGIN
                INSERT INTO Lists (UserID, ItemID,ListID, ListName, Quantity, Price, Date) VALUES 
                (1, 0,1, 'Cart', 0, 11.00, GETDATE());     
            END;

            -- Check if the Users table is empty
            IF NOT EXISTS (SELECT 1 FROM Users)
            BEGIN
                INSERT INTO Users (Email, Password, Lists) VALUES 
                ('newuser@example.com','{hashedPassword}', 0);
            END;

            -- Check if the Produce table is empty
            IF NOT EXISTS (SELECT 1 FROM Produce)
            BEGIN
                INSERT INTO Produce (ItemID, Name, Unit, Price) VALUES 
                (1, 'Granny Smith Apples', '1kg', '5.50'), 
                (2, 'Fresh tomatoes', '500g', '5.90'), 
                (3, 'Watermelon', 'Whole', '6.60'), 
                (4, 'Cucumber', '1 whole', '1.90'), 
                (5, 'Red potato washed', '1kg', '4.00'), 
                (6, 'Red tipped bananas', '1kg', '4.90'), 
                (7, 'Red onion', '1kg', '3.50'), 
                (8, 'Carrots', '1kg', '2.00'), 
                (9, 'Iceburg Lettuce', '1', '2.50'), 
                (10, 'Helga''s Wholemeal', '1', '3.70'), 
                (11, 'Free range chicken', '1kg', '7.50'), 
                (12, 'Manning Valley 6-pk', '6 eggs', '3.60'), 
                (13, 'A2 light milk', '1 litre', '2.90'), 
                (14, 'Chobani Strawberry Yoghurt', '1', '1.50'), 
                (15, 'Lurpak Salted Blend', '250g', '5.00'), 
                (16, 'Bega Farmers Tasty', '250g', '4.00'), 
                (17, 'Babybel Mini', '100g', '4.20'), 
                (18, 'Cobram EVOO', '375ml', '8.00'), 
                (19, 'Heinz Tomato Soup', '535g', '2.50'), 
                (20, 'John West Tuna can', '95g', '1.50'), 
                (21, 'Cadbury Dairy Milk', '200g', '5.00'), 
                (22, 'Coca Cola', '2 litre', '2.85'), 
                (23, 'Smith''s Original Share Pack Crisps', '170g', '3.29'), 
                (24, 'Birds Eye Fish Fingers', '375g', '4.50'), 
                (25, 'Berri Orange Juice', '2 litre', '6.00'), 
                (26, 'Vegemite', '380g', '6.00'), 
                (27, 'Cheddar Shapes', '175g', '2.00'), 
                (28, 'Colgate Total Toothpaste Original', '110g', '3.50'), 
                (29, 'Milo Chocolate Malt', '200g', '4.00'), 
                (30, 'Weet Bix Saniatarium Organic', '750g', '5.33'), 
                (31, 'Lindt Excellence 70% Cocoa Block', '100g', '4.25'), 
                (32, 'Original Tim Tams Choclate', '200g', '3.65'), 
                (33, 'Philadelphia Original Cream Cheese', '250g', '4.30'), 
                (34, 'Moccana Classic Instant Medium Roast', '100g', '6.00'), 
                (35, 'Capilano Squeezable Honey', '500g', '7.35'), 
                (36, 'Nutella jar', '400g', '4.00'), 
                (37, 'Arnott''s Scotch Finger', '250g', '2.85'), 
                (38, 'South Cape Greek Feta', '200g', '5.00'), 
                (39, 'Sacla Pasta Tomato Basil Sauce', '420g', '4.50'), 
                (40, 'Primo English Ham', '100g', '3.00'), 
                (41, 'Primo Short cut rindless Bacon', '175g', '5.00'), 
                (42, 'Golden Circle Pineapple Pieces in natural juice', '440g', '3.25'), 
                (43, 'San Remo Linguine Pasta No 1', '500g', '1.95');
            END;";

                    SqlCommand checkAndPopulateTablesCommand = new SqlCommand(checkAndPopulateTablesQuery, connection);
                    checkAndPopulateTablesCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // Log the exception
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
            }
        }


        #endregion
    #region GET
       
        public UserDTO GetUserById(int userId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.QueryFirstOrDefault<UserDTO>("SELECT UserID, Email,Password,Lists FROM Users WHERE UserID = @UserID", new { UserID = userId });
            }
        }
        [HttpPost]
        public UserDTO GetUserByEmail(string email)
        {
            string cleanedEmail = email.Trim();
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.QueryFirstOrDefault<UserDTO>("SELECT UserID, Email, Password ,Lists FROM Users WHERE Email = @Email", new { Email = cleanedEmail });
            }
        }     
        public int GetMaxListIdForUser(int userId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                int maxListId = dbConnection.QuerySingleOrDefault<int>(
                    "SELECT ISNULL(MAX(ListID), 0) FROM Lists WHERE UserID = @UserId",
                    new { UserId = userId }
                );
                return maxListId + 1;
            }
        }
        public int GetNewListIdForAll()
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                int maxListId = dbConnection.QuerySingleOrDefault<int>(
                    "SELECT ISNULL(MAX(ListID), 0) FROM Lists"
                    
                );
                return maxListId + 1;
            }
        }
        public int GetCurrentListIdForUser(int userId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                int maxListId = dbConnection.QuerySingleOrDefault<int>(
                    "SELECT ISNULL(MAX(ListID), 0) FROM Lists WHERE UserID = @UserId",
                    new { UserId = userId }
                );
                return maxListId;
            }
        }
        public int GetMaxUserId()
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                int maxUserId = dbConnection.QuerySingleOrDefault<int>(
                    "SELECT ISNULL(MAX(UserID), 0) FROM Users"
                );
                return maxUserId;
            }
        }
        public List<ListDTO> GetListById(int userID)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.Query<ListDTO>("SELECT ListID, UserID, ItemID, FinalisedDate FROM Lists WHERE UserID = @UserID", new { UserID = userID }).ToList();
            }
        }
        public bool HasListsWithoutFinalisedDate(int userID)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();

                // Query to check if there are any lists without a FinalisedDate for the given user
                var result = dbConnection.QuerySingleOrDefault<int>(
                    "SELECT COUNT(*) FROM Lists WHERE UserID = @UserID AND FinalisedDate IS NULL",
                    new { UserID = userID }
                );

                // If result is greater than 0, that means there are lists without a FinalisedDate, so return false
                return result == 0;
            }
        }
        public List<ProduceDTO> GetProduceByItemName(string itemName)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                string query = "SELECT ItemID, Name, Unit, Price FROM Produce WHERE Name LIKE @Name";
                return dbConnection.Query<ProduceDTO>(query, new { Name = "%" + itemName + "%" }).ToList();
            }
        }
        public List<ProduceDTO> GetProduce()
        {
            List<ProduceDTO> produce = new List<ProduceDTO>();

            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                var result = dbConnection.Query<ProduceDTO>("SELECT ItemID, Name, Unit, Price FROM Produce");
                foreach (var item in result)
                {
                    produce.Add(item);
                }
            }

            return produce;
        }
        #endregion
    #region SET

        // Set method for UserDTO
        public void SetUser(UserDTO user)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                dbConnection.Execute("INSERT INTO Users (Email,Lists,Password) VALUES (@Email,@Lists,@Password)", user);
            }
        }

        // Set method for ListDTO
        public void SetList(ListDTO list)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                dbConnection.Execute("INSERT INTO Lists (ListID, UserID, ItemID,ListName,Quantity,Price) VALUES (@ListID, @UserID, @ItemID,@ListName,@Quantity,@Price)", list);
            }
        }

        // Set method for ProduceDTO
        public void SetProduce(ProduceDTO produce)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                dbConnection.Execute("INSERT INTO Produce (ItemID, Name, Unit, Price) VALUES (@ItemID, @Name, @Unit, @Price)", produce);
            }
        }
        #endregion
    #region PUT

        // Put method for UserDTO
        public void PutUser(UserDTO user)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                dbConnection.Execute("UPDATE Users SET Email = @Email, Lists = @Lists, Password = @Password WHERE UserID = @UserID", user);
            }
        }

        // Put method for ListDTO
        public void PutList(ListDTO list)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                dbConnection.Execute("UPDATE Lists SET UserID = @UserID, ItemID = @ItemID WHERE ListID = @ListID", list);
            }
        }

        // Put method for ProduceDTO
        public void PutProduce(ProduceDTO produce)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                dbConnection.Execute("UPDATE Produce SET Name = @Name, Unit = @Unit, Price = @Price WHERE ItemID = @ItemID", produce);
            }
        }


        #endregion
    #region POST
        public void PostUser(UserDTO user)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                dbConnection.Execute("INSERT INTO Users (Email,Lists) VALUES (@Email,@Lists)", user);
            }
        }

  
        #endregion
    #region DELETE
        // Delete method for UserDTO
        public void DeleteUser(int userId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                dbConnection.Execute("DELETE FROM Users WHERE UserID = @UserID", new { UserID = userId });
            }
        }

        // Delete method for ListDTO
        public void DeleteList(int listId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                dbConnection.Execute("DELETE FROM Lists WHERE ListID = @ListID", new { ListID = listId });
            }
        }

        // Delete method for ProduceDTO
        public void DeleteProduce(int listId, int itemId, int userId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();

                // Delete the produce item from the database based on listId, itemId, and userId
                string deleteQuery = "DELETE FROM Lists WHERE ListID = @ListID AND ItemID = @ItemID AND UserID = @UserID";
                dbConnection.Execute(deleteQuery, new { ListID = listId, ItemID = itemId, UserID = userId });
            }
        }



        #endregion
    #region CART MECHANICS
        public IEnumerable<int> GetUserLists(int userId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                var query = @"SELECT DISTINCT
                        ListID
                      FROM Lists
                      WHERE UserID = @UserId";
                return dbConnection.Query<int>(query, new { UserId = userId }).Take(1);
            }
        }
        public List<ListDTO> GetUserListsDTO(int userId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                var query = @"SELECT ListID, UserID, FinalisedDate, ListName 
                      FROM Lists 
                      WHERE UserID = @UserId"; // Ensure it filters by UserID
                return dbConnection.Query<ListDTO>(query, new { UserId = userId }).ToList();
            }
        }



        public List<ListDTO> GetListItems(int listId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                // Retrieve list items for the specified list ID
                return dbConnection.Query<ListDTO>("SELECT * FROM Lists WHERE ListID = @ListId", new { ListId = listId }).ToList();
            }
        }

        public void AddList(ListDTO listDTO)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                // Insert a new list for the user
                dbConnection.Execute("INSERT INTO Lists (UserID,ItemID,ListID,ListName,Price,Quantity) VALUES (@UserId,@ItemID,@ListID,@ListName,@Price,@Quantity)", listDTO);
            }
        }
        public void AddListFinalised(ListDTO listDTO)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                // Insert a new list for the user
                dbConnection.Execute("INSERT INTO Lists (UserID,ItemID,ListID,ListName,Price,Quantity,FinalisedDate) VALUES (@UserId,@ItemID,@ListID,@ListName,@Price,@Quantity,@FinalisedDate)", listDTO);
            }
        }
        public string GetListNameById(int listId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.QueryFirstOrDefault<string>("SELECT ListName FROM Lists WHERE ListID = @ListId", new { ListId = listId });
            }
        }
        public DateTime? GetListCreated(int listId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                // Query the Date column from the Lists table
                return dbConnection.QueryFirstOrDefault<DateTime?>(
                    "SELECT Date FROM Lists WHERE ListID = @ListId",
                    new { ListId = listId }
                );
            }
        }

        public void UpdateItemQuantity(int listId, int itemId, int additionalQuantity)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();

                // Retrieve current quantity, handle null values by providing a default value of 0
                string selectQuery = "SELECT ISNULL(Quantity, 0) AS Quantity FROM Lists WHERE ListID = @ListID AND ItemID = @ItemID";
                int currentQuantity = dbConnection.QuerySingleOrDefault<int>(selectQuery, new { ListID = listId, ItemID = itemId });

                // Calculate new quantity
                int newQuantity = currentQuantity + additionalQuantity;

                // Update Quantity in the database
                string updateQuery = "UPDATE Lists SET Quantity = @Quantity WHERE ListID = @ListID AND ItemID = @ItemID";
                dbConnection.Execute(updateQuery, new { Quantity = newQuantity, ListID = listId, ItemID = itemId });

                Console.WriteLine($"Updated ItemID {itemId} in ListID {listId} to new quantity: {newQuantity}");
            }
        }

        public bool DoesItemExistInCart(int listId, int itemId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();

                string query = "SELECT COUNT(1) FROM Lists WHERE ListID = @ListID AND ItemID = @ItemID";
                int count = dbConnection.QuerySingle<int>(query, new { ListID = listId, ItemID = itemId });

                return count > 0;
            }
        }

        public IEnumerable<ListDTO> GetAllUserLists(int userId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.Query<ListDTO>("SELECT ListID, ListName, UserID FROM Lists WHERE UserID = @UserId", new { UserId = userId });
            }
        }

        public IEnumerable<ListDTO> GetAllUserListsFinalised(int userId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.Query<ListDTO>(
                    "SELECT ListID, ListName, UserID, FinalisedDate, ItemID,Price,Quantity FROM Lists WHERE UserID = @UserId",
                    new { UserId = userId });
            }
        }
        public IEnumerable<ListDTO> FindUnsavedUserLists(int userId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();

                // Retrieve all unfinalized lists for the user
                var unfinalizedLists = dbConnection.Query<ListDTO>(
                    "SELECT ListID, ListName, UserID, FinalisedDate FROM Lists WHERE UserID = @UserId AND FinalisedDate IS NULL",
                    new { UserId = userId }).ToList();

                // If there are any unfinalized lists, update their FinalisedDate
                if (unfinalizedLists.Any())
                {
                    // Update the FinalisedDate to current date for all unfinalized lists
                    var now = DateTime.Now;
                    var listIds = unfinalizedLists.Select(list => list.ListID).ToArray();

                    dbConnection.Execute(
                        "UPDATE Lists SET FinalisedDate = @Now WHERE ListID IN @ListIds",
                        new { Now = now, ListIds = listIds });
                }

                return unfinalizedLists;
            }
        }



        public IEnumerable<ProduceDTO> GetUserListProducts(int itemID)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                // Retrieve products for the specified list ID
                return dbConnection.Query<ProduceDTO>("SELECT * FROM Produce WHERE ItemID = @ItemID", new { ItemID = itemID });
            }
        }
        public string GetUserEmail(int userId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                // Retrieve the user email for the specified user ID
                return dbConnection.QuerySingleOrDefault<string>("SELECT Email FROM Users WHERE UserID = @UserId", new { UserId = userId });
            }
        }
        public int GetItemQuantityInList(int listId, int itemId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();

                string query = "SELECT Quantity FROM Lists WHERE ListID = @ListID AND ItemID = @ItemID";

                return dbConnection.QuerySingleOrDefault<int>(query, new { ListID = listId, ItemID = itemId });
            }
        }
        public int GetMaxListIndex(int listId)
        {
            int maxListIndex = 0;

            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                string query = "SELECT MAX(ListIndex) FROM ListDTO WHERE ListID = @ListId";
                maxListIndex = dbConnection.QueryFirstOrDefault<int?>(query, new { ListId = listId }) ?? 0;
            }

            return maxListIndex;
        }
        public string GetProductName(int productId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.ExecuteScalar<string>("SELECT Name FROM Produce WHERE ItemID = @ProductId", new { ProductId = productId });
            }
        }

        public decimal GetProductPrice(int productId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.ExecuteScalar<decimal>("SELECT Price FROM Produce WHERE ItemID = @ProductId", new { ProductId = productId });
            }
        }

        public string GetProductWeight(int productId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.ExecuteScalar<string>("SELECT Unit FROM Produce WHERE ItemID = @ProductId", new { ProductId = productId });
            }
        }
        public void UpdateCartPrice(int listId, int itemId, int quantity)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();

        
                var product = GetUserListProducts(itemId).FirstOrDefault();

                if (product != null)
                {
                    // Calculate the new price
                    decimal newPriceint = product.Price * quantity;
                    string newPrice = Convert.ToString(newPriceint);
                    // Update the price in the cart
                    string updateQuery = "UPDATE Lists SET Price = @NewPrice WHERE ListID = @ListID AND ItemID = @ItemID";
                    dbConnection.Execute(updateQuery, new { NewPrice = newPrice, ListID = listId, ItemID = itemId });
                }
            }
        }
        public decimal GetProductPriceByItemId(int itemId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();

                // Retrieve the price of the product for the specified ItemID
                string query = "SELECT Price FROM Lists WHERE ItemID = @ItemID";
                var price = dbConnection.QueryFirstOrDefault<decimal?>(query, new { ItemID = itemId });

                return price ?? 0; // Return 0 if price is null
            }
        }




        public void UpdateListName(int listId, string newListName)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                string query = "UPDATE Lists SET ListName = @NewListName WHERE ListID = @ListID";
                dbConnection.Execute(query, new { NewListName = newListName, ListID = listId });
            }
        }



        #endregion
    }
}