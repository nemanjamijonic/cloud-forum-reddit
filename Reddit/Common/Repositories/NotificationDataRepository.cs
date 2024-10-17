using Common.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Queryable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public class NotificationDataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public NotificationDataRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("NotificationDataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("NotificationTable");
            _table.CreateIfNotExists();
        }

        public IQueryable<Notification> GetNotificationsForPost(Guid postId)
        {
            var notifications = from g in _table.CreateQuery<Notification>()
                           where g.PartitionKey == "Notification" && g.PostId == postId
                           select g;
            return notifications;
        }

        public async Task<List<Notification>> GetNotificationsForSpecificPost(Guid postId)
        {
            var query = (from g in _table.CreateQuery<Notification>()
                         where g.PartitionKey == "Notification" && g.PostId == postId
                         select g).AsTableQuery();

            var segment = await query.ExecuteSegmentedAsync(null);
            return segment.Results.ToList();
        }



        public async Task<bool> DoesNotificationExist(Guid userId, Guid postId)
        {
            string userIdFilter = TableQuery.GenerateFilterCondition("UserId", QueryComparisons.Equal, userId.ToString());
            string postIdFilter = TableQuery.GenerateFilterCondition("PostId", QueryComparisons.Equal, postId.ToString());
            string combinedFilter = TableQuery.CombineFilters(userIdFilter, TableOperators.And, postIdFilter);

            TableQuery<Notification> query = new TableQuery<Notification>().Where(combinedFilter);

            var result = await _table.ExecuteQuerySegmentedAsync(query, null);
            return result.Results.Any();
        }

        public async Task<Notification> GetNotification(Guid userId, Guid postId)
        {
            var query = (from g in _table.CreateQuery<Notification>()
                         where g.PartitionKey == "Notification" && g.PostId == postId && g.UserId == userId
                         select g).AsTableQuery();

            var segment = await query.ExecuteSegmentedAsync(null);
            return segment.Results.FirstOrDefault();
        }



        public async Task CreatePost(Notification newNotification)
        {
            TableOperation insertOperation = TableOperation.Insert(newNotification);
            await _table.ExecuteAsync(insertOperation);
        }

        public async Task UpdatePost(Notification newNotification)
        {
            TableOperation updateOperation = TableOperation.Replace(newNotification);
            await _table.ExecuteAsync(updateOperation);
        }
    }
}
