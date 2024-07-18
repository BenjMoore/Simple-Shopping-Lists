using ICTPRG535_556.Models;
using Dapper;
using System.Collections;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DataMapper
{
    public class DataAccess
    {
        private readonly string connectionString;

        public DataAccess()
        {
            this.connectionString = "Server=localhost;Database=ShoppingList;Trusted_Connection=True;TrustServerCertificate=True;";
        }
        #region GET
        public ArrayList GetUsers()
        {
            ArrayList users = new ArrayList();

            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                var result = dbConnection.Query<UserDTO>("SELECT UserID, Email FROM Users");
                foreach (var user in result)
                {
                    users.Add(user);
                }
            }

            return users;
        }
        public UserDTO GetUserById(int userId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.QueryFirstOrDefault<UserDTO>("SELECT UserID, Email,Lists FROM Users WHERE UserID = @UserID", new { UserID = userId });
            }
        }

        [HttpPost]
        public UserDTO GetUserByEmail(string email)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.QueryFirstOrDefault<UserDTO>("SELECT UserID, Email, Lists FROM Users WHERE Email = @Email", new { Email = email });
            }
        }
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


        public ListDTO GetFirstListByID(int userID)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.QueryFirstOrDefault<ListDTO>("SELECT ListID, UserID, ItemID FROM Lists WHERE UserID = @UserID", new { UserID = userID });
            }
        }
        public List<ListDTO> GetListById(int userID)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.Query<ListDTO>("SELECT ListID, UserID, ItemID FROM Lists WHERE UserID = @UserID", new { UserID = userID }).ToList();
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
                dbConnection.Execute("INSERT INTO Lists (UserID,ListID,ItemID,ListName,Price) VALUES (@UserId,@ListID,@ItemID,@ListName,@Price)", listDTO);
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
        public IEnumerable<ListDTO> GetAllUserLists(int userId)
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.Query<ListDTO>("SELECT ListID, ListName, UserID FROM Lists WHERE UserID = @UserId", new { UserId = userId });
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

                // Retrieve the product price using the existing method
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

                // Log the price for debugging
                if (price == null)
                {
                    Console.WriteLine($"Price for ItemID {itemId} not found.");
                }
                else
                {
                    Console.WriteLine($"Price for ItemID {itemId}: {price.Value}");
                }

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