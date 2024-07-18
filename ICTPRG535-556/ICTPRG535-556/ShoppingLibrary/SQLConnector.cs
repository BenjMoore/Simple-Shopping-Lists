using Dapper;
using ICTPRG535_556.Models;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Data;



namespace ShoppingLibrary
{
   
    public class DataAccess
    {
        private readonly string connectionString;

        public DataAccess(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IEnumerable<UserDTO> GetUsers()
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.Query<UserDTO>("SELECT UserID, Email FROM Users");
            }
        }

        public IEnumerable<ListDTO> GetLists()
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.Query<ListDTO>("SELECT ListID, UserID, ItemID FROM Lists");
            }
        }

        public IEnumerable<ProduceDTO> GetProduce()
        {
            using (IDbConnection dbConnection = new SqlConnection(connectionString))
            {
                dbConnection.Open();
                return dbConnection.Query<ProduceDTO>("SELECT ItemID, Name, Unit, Price FROM Produce");
            }
        }
    }

}
