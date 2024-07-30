using ICTPRG535_556.Models;
using Dapper;
using System.Collections;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DataMapper
{
    // SQL Interface DataAccess - Benjamin Moore
    public class DataAccess
    {
        private readonly string connectionString;
        private readonly string noDBOConnectionstring;

        public DataAccess()
        {
            // Main Connection String
            this.connectionString = "Server=localhost;Database=ShoppingList;Trusted_Connection=True;TrustServerCertificate=True;";
            // DB Initialisation Connection String
            this.noDBOConnectionstring = "Server=localhost;Trusted_Connection=True;TrustServerCertificate=True;";
        }
        #region Initialise Database
        // Initialises database before operation
        public void InitializeDatabase()
        {

            // Create database if it doesn't exist
            CreateDatabase();

            // Create tables if they don't exist
            CreateTableLists();
            CreateTableUsers();
            CreateTableProduce();
            // Populate tables if empty
            if (IsTableEmpty("Lists"))
            {
                PopulateLists();
            }
            if (IsTableEmpty("Produce"))
            {
                PopulateProduce();
            }
            if (IsTableEmpty("Users"))
            {
                PopulateUsers();
            }

        }
        // Checks if the table name has content in the database with validation
        private bool IsTableEmpty(string tableName)
        {
            SqlConnection connection = new SqlConnection(connectionString);

            // Basic validation for table name
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name cannot be null or empty.", nameof(tableName));
            }

            // Query to count rows in the table
            string query = $"SELECT COUNT(*) FROM {tableName}";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                try
                {
                    connection.Open(); // Ensure the connection is open
                    int count = (int)command.ExecuteScalar();
                    return count == 0;
                }
                catch (Exception ex)
                {
                    // Log the exception if necessary
                    Console.WriteLine($"Error checking if table is empty: {ex.Message}");
                    return false;
                }
            }
        }
        // Creates the database if not exists
        private void CreateDatabase()
        {
            using (SqlConnection connection = new SqlConnection(noDBOConnectionstring))
            {
                string query = @"
            IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ShoppingList')
            BEGIN
                CREATE DATABASE ShoppingList;
            END;";

                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        // Creates the Lists table if not exists
        private void CreateTableLists()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Lists' AND xtype = 'U')
            BEGIN
                CREATE TABLE Lists (
                    ListID INT,
                    UserID INT NOT NULL,
                    ItemID INT NOT NULL,
                    ListName NCHAR(50),
                    ListIndex INT PRIMARY KEY IDENTITY(1,1) NOT NULL,
                    Quantity INT NOT NULL,
                    Price FLOAT NOT NULL
                   
                );
            END;";

                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        // Creates the Users table if not exists
        private void CreateTableUsers()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Users' AND xtype = 'U')
            BEGIN
                CREATE TABLE Users (
                    UserID INT PRIMARY KEY,
                    Email NCHAR(50) NOT NULL,
                    Lists INT NOT NULL
                );
            END;";

                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        // Creates the Produce table if not exists
        private void CreateTableProduce()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Produce' AND xtype = 'U')
            BEGIN
                CREATE TABLE Produce (
                    ItemID INT PRIMARY KEY,
                    Name NCHAR(60) NOT NULL,
                    Unit NCHAR(30) NOT NULL,
                    Price VARCHAR(30) NOT NULL
                );
            END;";

                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        // Populates The List Table 
        private void PopulateLists()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            INSERT INTO Lists (ListID,UserID, ItemID, ListName, Quantity, Price) VALUES 
            (0,1, 1, 'Example List', 2, 11.00);";

                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        // Populates The Users Table 
        private void PopulateUsers()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            INSERT INTO Users (UserID, Email, Lists) VALUES 
            (1,'newuser@example.com', 1);";

                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
        // Populates The Produce Table 
        private void PopulateProduce()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"
            INSERT INTO Produce (ItemID, Name, Unit, Price) VALUES 
            (1, 'Granny Smith Apples', '1kg', 5.50),
            (2, 'Fresh tomatoes', '500g', 5.90),
            (3, 'Watermelon', 'Whole', 6.60),
            (4, 'Cucumber', '1 whole', 1.90),
            (5, 'Red potato washed', '1kg', 4.00),   
            (6, 'Red tipped bananas', '1kg', 4.90),
            (7, 'Red onion', '1kg', 3.50),
            (8, 'Carrots', '1kg', 2.00),
            (9, 'Iceburg Lettuce', '1', 2.50),
            (10, 'Helga''s Wholemeal', '1', 3.70),
            (11, 'Free range chicken', '1kg', 7.50),
            (12, 'Manning Valley 6-pk', '6 eggs', 3.60),
            (13, 'A2 light milk', '1 litre', 2.90),
            (14, 'Chobani Strawberry Yoghurt', '1', 1.50),
            (15, 'Lurpak Salted Blend', '250g', 5.00),
            (16, 'Bega Farmers Tasty', '250g', 4.00),
            (17, 'Babybel Mini', '100g', 4.20),
            (18, 'Cobram EVOO', '375ml', 8.00),
            (19, 'Heinz Tomato Soup', '535g', 2.50),
            (20, 'John West Tuna can', '95g', 1.50),
            (21, 'Cadbury Dairy Milk', '200g', 5.00),
            (22, 'Coca Cola', '2 litre', 2.85),
            (23, 'Smith''s Original Share Pack Crisps', '170g', 3.29),
            (24, 'Birds Eye Fish Fingers', '375g', 4.50),
            (25, 'Berri Orange Juice', '2 litre', 6.00),
            (26, 'Vegemite', '380g', 6.00),
            (27, 'Cheddar Shapes', '175g', 2.00),
            (28, 'Colgate Total Toothpaste Original', '110g', 3.50),
            (29, 'Milo Chocolate Malt', '200g', 4.00),
            (30, 'Weet Bix Saniatarium Organic', '750g', 5.33),
            (31, 'Lindt Excellence 70% Cocoa Block', '100g', 4.25),
            (32, 'Original Tim Tams Choclate', '200g', 3.65),
            (33, 'Philadelphia Original Cream Cheese', '250g', 4.30),
            (34, 'Moccana Classic Instant Medium Roast', '100g', 6.00),
            (35, 'Capilano Squeezable Honey', '500g', 7.35),
            (36, 'Nutella jar', '400g', 4.00),
            (37, 'Arnott''s Scotch Finger', '250g', 2.85),
            (38, 'South Cape Greek Feta', '200g', 5.00),
            (39, 'Sacla Pasta Tomato Basil Sauce', '420g', 4.50),
            (40, 'Primo English Ham', '100g', 3.00),
            (41, 'Primo Short cut rindless Bacon', '175g', 5.00),
            (42, 'Golden Circle Pineapple Pieces in natural juice', '440g', 3.25),
            (43, 'San Remo Linguine Pasta No 1', '500g', 1.95);
";

                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        #endregion
        #region GET
        // Gets user by user id and returns them as a UserDTO
        public UserDTO GetUserById(int userId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.QueryFirstOrDefault<UserDTO>("SELECT UserID, Email,Lists FROM Users WHERE UserID = @UserID", new { UserID = userId });
            }
        }
        // Gets User by email and returns as UserDTO
        [HttpPost]
        public UserDTO GetUserByEmail(string email)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.QueryFirstOrDefault<UserDTO>("SELECT UserID, Email, Lists FROM Users WHERE Email = @Email", new { Email = email });
            }
        }
        // Gets All Lists and returns as an arraylist
        public ArrayList GetLists()
        {
            ArrayList lists = new ArrayList();

            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                var result = dbConnection.Query<ListDTO>("SELECT ListID, UserID, ItemID, ListName, Quantity FROM Lists");
                foreach (var list in result)
                {
                    lists.Add(list);
                }
            }

            return lists;
        }
        // Gets the highest ListID for user and then returns new max value to be assigned when creating a new list
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
        // Gets all lists by userID  and returns as a list of ListDTO
        public List<ListDTO> GetListById(int userID)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.Query<ListDTO>("SELECT ListID, UserID, ItemID FROM Lists WHERE UserID = @UserID", new { UserID = userID }).ToList();
            }
        }
        // Get Specific Produce where Item name is simular to the search term, used in search function
        public List<ProduceDTO> GetProduceByItemName(string itemName)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                string query = "SELECT ItemID, Name, Unit, Price FROM Produce WHERE Name LIKE @Name";
                return dbConnection.Query<ProduceDTO>(query, new { Name = "%" + itemName + "%" }).ToList();
            }
        }
        // Gets all produce 
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
                dbConnection.Execute("INSERT INTO Users (UserID, Email) VALUES (@UserID, @Email)", user);
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
                dbConnection.Execute("UPDATE Users SET Email = @Email, Lists = @Lists WHERE UserID = @UserID", user);
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
        // Adds User (Not implimented but useful for development)
        public void PostUser(UserDTO user)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                dbConnection.Execute("INSERT INTO Users (Email,Lists) VALUES (@Email,@Lists)", user);
            }
        }

        // Post method for ListDTO
        public void PostList(ListDTO list)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                dbConnection.Execute("INSERT INTO Lists (UserID, ItemID) VALUES (@UserID, @ItemID)", list);
            }
        }

        // Post method for ProduceDTO
        public void PostProduce(ProduceDTO produce)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                dbConnection.Execute("INSERT INTO Produce (Name, Unit, Price) VALUES (@Name, @Unit, @Price)", produce);
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

        // Retrieves a list of ListIDs associated with a specific user, limited to the first result.
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

        // Retrieves all items associated with a specific list.
        public List<ListDTO> GetListItems(int listId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.Query<ListDTO>("SELECT * FROM Lists WHERE ListID = @ListId", new { ListId = listId }).ToList();
            }
        }

        // Adds a new list to the database for a user.
        public void AddList(ListDTO listDTO)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                dbConnection.Execute("INSERT INTO Lists (UserID,ListID,ItemID,ListName,Quantity,Price) VALUES (@UserId,@ListID,@ItemID,@ListName,@Quantity,@Price)", listDTO);
            }
        }

        // Retrieves the name of a list by its ID.
        public string GetListNameById(int listId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.QueryFirstOrDefault<string>("SELECT ListName FROM Lists WHERE ListID = @ListId", new { ListId = listId });
            }
        }

        // Updates the quantity of an item in a list.
        public void UpdateItemQuantity(int listId, int itemId, int additionalQuantity)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();

                // Retrieve current quantity, handling null values with a default of 0
                string selectQuery = "SELECT ISNULL(Quantity, 0) AS Quantity FROM Lists WHERE ListID = @ListID AND ItemID = @ItemID";
                int currentQuantity = dbConnection.QuerySingleOrDefault<int>(selectQuery, new { ListID = listId, ItemID = itemId });

                int newQuantity = currentQuantity + additionalQuantity;

                string updateQuery = "UPDATE Lists SET Quantity = @Quantity WHERE ListID = @ListID AND ItemID = @ItemID";
                dbConnection.Execute(updateQuery, new { Quantity = newQuantity, ListID = listId, ItemID = itemId });

                Console.WriteLine($"Updated ItemID {itemId} in ListID {listId} to new quantity: {newQuantity}");
            }
        }

        // Checks if an item exists in a specified list.
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

        // Retrieves the number of items in a specified list.
        public int GetItemCountInList(int listId)
        {
            int itemCount = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM Lists WHERE ListID = @ListID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ListID", listId);
                    connection.Open();
                    itemCount = (int)command.ExecuteScalar();
                }
            }

            return itemCount;
        }

        // Retrieves all lists associated with a specific user.
        public IEnumerable<ListDTO> GetAllUserLists(int userId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.Query<ListDTO>("SELECT ListID, ListName, UserID FROM Lists WHERE UserID = @UserId", new { UserId = userId });
            }
        }

        // Retrieves products associated with a specified item ID.
        public IEnumerable<ProduceDTO> GetUserListProducts(int itemID)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.Query<ProduceDTO>("SELECT * FROM Produce WHERE ItemID = @ItemID", new { ItemID = itemID });
            }
        }

        // Retrieves the email address of a user by their ID.
        public string GetUserEmail(int userId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.QuerySingleOrDefault<string>("SELECT Email FROM Users WHERE UserID = @UserId", new { UserId = userId });
            }
        }

        // Retrieves the quantity of a specific item in a list.
        public int GetItemQuantityInList(int listId, int itemId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();

                string query = "SELECT Quantity FROM Lists WHERE ListID = @ListID AND ItemID = @ItemID";

                return dbConnection.QuerySingleOrDefault<int>(query, new { ListID = listId, ItemID = itemId });
            }
        }

        // Retrieves the highest index value in a list.
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

        // Retrieves the name of a product by its ID.
        public string GetProductName(int productId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.ExecuteScalar<string>("SELECT Name FROM Produce WHERE ItemID = @ProductId", new { ProductId = productId });
            }
        }

        // Retrieves the price of a product by its ID.
        public decimal GetProductPrice(int productId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.ExecuteScalar<decimal>("SELECT Price FROM Produce WHERE ItemID = @ProductId", new { ProductId = productId });
            }
        }

        // Retrieves the weight or unit of measurement of a product by its ID.
        public string GetProductWeight(int productId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.ExecuteScalar<string>("SELECT Unit FROM Produce WHERE ItemID = @ProductId", new { ProductId = productId });
            }
        }

        // Updates the total price of a specific item in the cart based on quantity.
        public void UpdateCartPrice(int listId, int itemId, int quantity)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();

                var product = GetUserListProducts(itemId).FirstOrDefault();

                if (product != null)
                {
                    decimal newPriceint = product.Price * quantity;
                    string newPrice = Convert.ToString(newPriceint);

                    string updateQuery = "UPDATE Lists SET Price = @NewPrice WHERE ListID = @ListID AND ItemID = @ItemID";
                    dbConnection.Execute(updateQuery, new { NewPrice = newPrice, ListID = listId, ItemID = itemId });
                }
            }
        }

        // Retrieves the price of a product based on the item ID.
        public decimal GetProductPriceByItemId(int itemId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();

                string query = "SELECT Price FROM Lists WHERE ItemID = @ItemID";
                var price = dbConnection.QueryFirstOrDefault<decimal?>(query, new { ItemID = itemId });

                if (price == null)
                {
                    Console.WriteLine($"Price for ItemID {itemId} not found.");
                }
                else
                {
                    Console.WriteLine($"Price for ItemID {itemId}: {price.Value}");
                }

                return price ?? 0;
            }
        }

        // Updates the name of a list.
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