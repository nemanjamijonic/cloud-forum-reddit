using Common.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public class PostDataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public PostDataRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("PostDataConnectionString"));
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("PostTable");
            _table.CreateIfNotExists();
        }


        public async Task<Post> GetPostById(string id)
        {
            TableQuery<Post> query = new TableQuery<Post>().Where(
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Post"),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id)
                )
            );

            TableQuerySegment<Post> queryResult = await _table.ExecuteQuerySegmentedAsync(query, null);
            return queryResult.FirstOrDefault();
        }


        public async Task IncrementLikes(string postId)
        {
            var post = await GetPost(postId);
            if (post != null)
            {
                post.Likes += 1;
                await UpdatePost(post);
            }
        }

        public async Task IncrementDislikes(string postId)
        {
            var post = await GetPost(postId);
            if (post != null)
            {
                post.Dislikes += 1;
                await UpdatePost(post);
            }
        }

        public async Task DecrementLikes(string postId)
        {
            var post = await GetPost(postId);
            if (post != null && post.Likes > 0)
            {
                post.Likes -= 1;
                await UpdatePost(post);
            }
        }

        public async Task DecrementDislikes(string postId)
        {
            var post = await GetPost(postId);
            if (post != null && post.Dislikes > 0)
            {
                post.Dislikes -= 1;
                await UpdatePost(post);
            }
        }


        public async Task CreatePost(Post newPost)
        {
            TableOperation insertOperation = TableOperation.Insert(newPost);
            await _table.ExecuteAsync(insertOperation);
        }

        public List<Post> RetrieveAllPosts()
        {
            var results = from g in _table.CreateQuery<Post>()
                          where g.PartitionKey == "Post" && !g.IsDeleted
                          select g;

            return results.ToList();
        }

        public List<Post> RetrieveAllPostsForUser(string userId)
        {
            var results = from g in _table.CreateQuery<Post>()
                          where g.PartitionKey == "Post" && !g.IsDeleted && g.UserId == Guid.Parse(userId)
                          select g;

            return results.ToList();
        }


        public async Task<Post> GetPost(string postId)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<Post>("Post", postId);
            TableResult retrievedResult = await _table.ExecuteAsync(retrieveOperation);
            return retrievedResult.Result as Post;
        }

        public async Task IncrementCommentCount(string postId)
        {
            var post = await GetPost(postId);
            if (post != null)
            {
                post.CommentsNumber += 1;
                await UpdatePost(post);
            }
        }


        public async Task UpdatePost(Post updatedPost)
        {
            TableOperation updateOperation = TableOperation.Replace(updatedPost);
            await _table.ExecuteAsync(updateOperation);
        }


        public async Task DeletePost(string postId, string loggedInUserId)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<Post>("Post", postId);
            TableResult retrievedResult = await _table.ExecuteAsync(retrieveOperation);
            if (retrievedResult.Result != null)
            {
                var post = (Post)retrievedResult.Result;
                if (post.UserId != Guid.Parse(loggedInUserId))
                {
                    throw new Exception("You are not authorized to delete this post.");
                }

                post.IsDeleted = true;

                TableOperation updateOperation = TableOperation.Replace(post);
                await _table.ExecuteAsync(updateOperation);
            }
            else
            {
                throw new Exception("Post not found.");
            }
        }
    }
}
