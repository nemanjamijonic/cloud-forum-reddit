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
    public class CommentDataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public CommentDataRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("CommentDataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("CommentTable"); _table.CreateIfNotExists();
        }

        public async Task<string> GetPostIdByCommentId(string commentId)
        {
            // Preuzimanje komentara koristeći prosleđeni commentId
            var comment = await GetComment(commentId);

            // Ako komentar nije pronađen, vraća null ili može baciti izuzetak
            if (comment == null)
            {
                throw new Exception($"Comment with ID {commentId} not found.");
            }

            // Vraćanje postId iz preuzetog komentara
            return comment.PostID;
        }



        public async Task UpdateComment(Comment updatedComment)
        {
            TableOperation updateOperation = TableOperation.Replace(updatedComment);
            await _table.ExecuteAsync(updateOperation);
        }

        public async Task<Comment> GetComment(string commentId)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<Comment>("Comment", commentId);
            TableResult retrievedResult = await _table.ExecuteAsync(retrieveOperation);
            return retrievedResult.Result as Comment;
        }

        public async Task IncrementLikes(string commentId)
        {
            var comment = await GetComment(commentId);
            if (comment != null)
            {
                comment.NumberOfLikes += 1;
                await UpdateComment(comment);
            }
        }

        public async Task IncrementDislikes(string commentId)
        {
            var comment = await GetComment(commentId);
            if (comment != null)
            {
                comment.NumberOfDislikes += 1;
                await UpdateComment(comment);
            }
        }

        public async Task DecrementLikes(string commentId)
        {
            var comment = await GetComment(commentId);
            if (comment != null && comment.NumberOfLikes > 0)
            {
                comment.NumberOfLikes -= 1;
                await UpdateComment(comment);
            }
        }

        public async Task DecrementDislikes(string commentId)
        {
            var comment = await GetComment(commentId);
            if (comment != null && comment.NumberOfDislikes > 0)
            {
                comment.NumberOfDislikes -= 1;
                await UpdateComment(comment);
            }
        }

        public async Task CreateComment(Comment newComment)
        {
            TableOperation insertOperation = TableOperation.Insert(newComment);
            await _table.ExecuteAsync(insertOperation);
        }

        public IQueryable<Comment> RetrieveCommentsForPost(string postId)
        {
            var comments = from g in _table.CreateQuery<Comment>()
                           where g.PartitionKey == "Comment" && g.PostID == postId && g.IsDeleted == false
                           select g;
            return comments;
        }

        public void DeleteComment(Guid commentId)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<Comment>("Comment", commentId.ToString());
            TableResult retrievedResult = _table.Execute(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                var comment = (Comment)retrievedResult.Result;
                comment.IsDeleted = true;

                TableOperation updateOperation = TableOperation.Replace(comment);
                _table.Execute(updateOperation);
            }
            else
            {
                throw new Exception("Comment not found.");
            }
        }

    }
}
