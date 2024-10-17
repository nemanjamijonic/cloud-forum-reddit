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
    public class PostReactionsDataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public PostReactionsDataRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("PostReactionsDataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("PostReactionsTable");
            _table.CreateIfNotExists();
        }

       

        public async Task<PostReactions> GetPostReactionAsync(Guid postId, Guid userId)
        {
            var query = new TableQuery<PostReactions>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "PostReactions"),
                    TableOperators.And,
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterConditionForGuid("PostId", QueryComparisons.Equal, postId),
                        TableOperators.And,
                        TableQuery.GenerateFilterConditionForGuid("UserId", QueryComparisons.Equal, userId)
                    )
                ));

            var result = await _table.ExecuteQuerySegmentedAsync(query, null);
            return result.Results.FirstOrDefault();
        }

        public async Task InsertOrUpdatePostReactionAsync(PostReactions postReaction)
        {
            var operation = TableOperation.InsertOrReplace(postReaction);
            await _table.ExecuteAsync(operation);
        }

    }
}
