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
    public class HealthCheckDataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public HealthCheckDataRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("HealthCheckDataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("HealthCheckTable");
            _table.CreateIfNotExists();
        }

        public async Task LogHealthCheck(HealthCheck newHealthCheck)
        {
            TableOperation insertOperation = TableOperation.Insert(newHealthCheck);
            await _table.ExecuteAsync(insertOperation);
        }

        public async Task<List<HealthCheck>> GetRedditServiceHealthChecksAsync(DateTime fromTime)
        {
            TableQuery<HealthCheck> query = new TableQuery<HealthCheck>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterConditionForDate("CheckedAt", QueryComparisons.GreaterThanOrEqual, fromTime),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("ServiceUrl", QueryComparisons.Equal, "RedditService")
                )
            );

            TableContinuationToken token = null;
            var results = new List<HealthCheck>();
            do
            {
                var queryResult = await _table.ExecuteQuerySegmentedAsync(query, token);
                results.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            // Sort results by CheckedAt in descending order
            return results.OrderByDescending(c => c.CheckedAt).ToList();
        }

        public async Task<List<HealthCheck>> GetNotificationServiceHealthChecksAsync(DateTime fromTime)
        {
            TableQuery<HealthCheck> query = new TableQuery<HealthCheck>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterConditionForDate("CheckedAt", QueryComparisons.GreaterThanOrEqual, fromTime),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("ServiceUrl", QueryComparisons.Equal, "NotificationService")
                )
            );

            TableContinuationToken token = null;
            var results = new List<HealthCheck>();
            do
            {
                var queryResult = await _table.ExecuteQuerySegmentedAsync(query, token);
                results.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            // Sort results by CheckedAt in descending order
            return results.OrderByDescending(c => c.CheckedAt).ToList();
        }
    }
}
