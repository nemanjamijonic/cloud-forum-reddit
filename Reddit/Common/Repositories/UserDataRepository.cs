using Common.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public class UserDataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public UserDataRepository() 
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("UserDataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("UserTable"); _table.CreateIfNotExists();
        }



        public async Task<int> GetTableRowCount()
        {
            try
            {
                // Formirajte upit za brojanje redova
                var query = new TableQuery<User>().Where(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "User"))
                    .Select(new string[] { "RowKey" });

                // Izvršite upit nad tabelom
                var queryResult = await _table.ExecuteQuerySegmentedAsync(query, null);

                // Broj redova je jednak broju rezultata upita
                return queryResult.Results.Count;
            }
            catch (Exception ex)
            {
                // Uhvatite eventualne greške prilikom izvršavanja upita
                Console.WriteLine($"Greška prilikom dobijanja broja redova u tabeli: {ex.Message}");
                throw;
            }
        }

        public void CreateUser(User newUser)
        {
            try
            {
                newUser.Username = newUser.Email;
                if (!_table.Exists())
                {

                    _table.Create();
                }

                TableOperation insertOperation = TableOperation.Insert(newUser);
                _table.ExecuteAsync(insertOperation);
            }
            catch (StorageException ex)
            {

                Console.WriteLine($"Greška prilikom kreiranja korisnika: {ex.Message}");
                throw;
            }
        }

        public async Task<User> GetUserById(string id)
        {
            TableQuery<User> query = new TableQuery<User>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "User"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id)
                )
            );

            TableQuerySegment<User> queryResult = await _table.ExecuteQuerySegmentedAsync(query, null);
            return queryResult.FirstOrDefault();
        }

        public async Task<User> GetUserByUsernameAndPassword(string email, string hashPassword)
        {
            TableQuery<User> query = new TableQuery<User>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "User"),
                    TableOperators.And,
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("Email", QueryComparisons.Equal, email),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("Password", QueryComparisons.Equal, hashPassword)
                    )
                )
            );

            TableQuerySegment<User> queryResult = await _table.ExecuteQuerySegmentedAsync(query, null);
            return queryResult.FirstOrDefault();
        }


        public async Task<User> GetUserByUsername(string username)
        {
            TableQuery<User> query = new TableQuery<User>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "User"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("Username", QueryComparisons.Equal, username)
                )
            );

            TableQuerySegment<User> queryResult = await _table.ExecuteQuerySegmentedAsync(query, null);
            return queryResult.FirstOrDefault();
        }

        public async Task<User> GetUser(string username)
        {
            TableQuery<User> query = new TableQuery<User>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "User"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("Username", QueryComparisons.Equal, username)
                )
            );

            TableQuerySegment<User> queryResult = await _table.ExecuteQuerySegmentedAsync(query, null);
            return queryResult.Results.FirstOrDefault();
        }


        public async Task UpdateUser(User updatedUser)
        {
            try
            {
                // Retrieve the existing user
                TableOperation retrieveOperation = TableOperation.Retrieve<User>("User", updatedUser.RowKey);
                TableResult retrievedResult = await _table.ExecuteAsync(retrieveOperation);

                User existingUser = retrievedResult.Result as User;

                if (existingUser != null)
                {
                    // Update properties
                    existingUser.FirstName = updatedUser.FirstName;
                    existingUser.LastName = updatedUser.LastName;
                    existingUser.Address = updatedUser.Address;
                    existingUser.City = updatedUser.City;
                    existingUser.Country = updatedUser.Country;
                    existingUser.PhoneNumber = updatedUser.PhoneNumber;
                    existingUser.ImageUrl = updatedUser.ImageUrl;

                    // Create the Replace TableOperation
                    TableOperation updateOperation = TableOperation.Replace(existingUser);

                    // Execute the operation
                    await _table.ExecuteAsync(updateOperation);
                }
                else
                {
                    throw new Exception("User not found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                throw;
            }
        }


    }
}
