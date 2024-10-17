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
    public class CommentReactionsDataRepository
    {

        private CloudStorageAccount _storageAccount;
        private CloudTable _table;


        public CommentReactionsDataRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("CommentReactionsDataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("CommentReactionsTable");
            _table.CreateIfNotExists();
        }

        public async Task<CommentReactions> GetCommentReactionAsync(Guid commentId, Guid userId)
        {
            var query = new TableQuery<CommentReactions>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "CommentReactions"),
                    TableOperators.And,
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterConditionForGuid("CommentId", QueryComparisons.Equal, commentId),
                        TableOperators.And,
                        TableQuery.GenerateFilterConditionForGuid("UserId", QueryComparisons.Equal, userId)
                    )
                ));

            var result = await _table.ExecuteQuerySegmentedAsync(query, null);
            return result.Results.FirstOrDefault();
        }

        public async Task InsertOrUpdateCommentReactionAsync(CommentReactions commentReaction)
        {
            var operation = TableOperation.InsertOrReplace(commentReaction);
            await _table.ExecuteAsync(operation);
        }
    }
}
