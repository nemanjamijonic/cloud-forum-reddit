using Common.Blobs;
using Common.Models;
using Common.Queues;
using Common.Repositories;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;



namespace RedditWebRole.Controllers
{
    public class PostController : Controller
    {
        private PostDataRepository postDataRepository = new PostDataRepository();
        private CommentDataRepository commentDataRepository = new CommentDataRepository();
        private UserDataRepository userDataRepository = new UserDataRepository();
        private PostReactionsDataRepository postReactionsDataRepository = new PostReactionsDataRepository();
        private CommentReactionsDataRepository commentReactionsDataRepository = new CommentReactionsDataRepository();
        // GET: Post


        public ActionResult Index(string title, string sort)
        {
            var posts = postDataRepository.RetrieveAllPosts();

            if (!string.IsNullOrEmpty(title))
            {
                posts = posts.Where(p => p.Title.ToLower() != null && p.Title.IndexOf(title.ToLower(), StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            switch (sort)
            {   
                case "likes":
                    posts = posts.OrderByDescending(p => p.Likes).ToList();
                    break;
                case "dislikes":
                    posts = posts.OrderByDescending(p => p.Dislikes).ToList();
                    break;
                case "comments":
                    posts = posts.OrderByDescending(p => p.CommentsNumber).ToList();
                    break;
                default:
                    break;
            }

            TempData["FilteredSortedPosts"] = posts;
            return RedirectToAction("Index", "Home");
        }


        public async Task<ActionResult> SpecificPost(string id)
        {
            var post = await postDataRepository.GetPostById(id);
            post.Comments = commentDataRepository.RetrieveCommentsForPost(post.RowKey).ToList();
            post.CommentsNumber = post.Comments.Count();
            var loggedInUser = (User)Session["LoggedInUser"];
            ViewBag.LoggedInUsername = loggedInUser.Username;
            if (post == null)
            {
                return HttpNotFound();
            }
            return View(post);
        }

        [HttpPost]
        public JsonResult DeleteComment(Guid commentId)
        {
            try
            {
                // Log the received commentId for debugging
                System.Diagnostics.Debug.WriteLine($"Received commentId: {commentId}");

                commentDataRepository.DeleteComment(commentId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Log the exception message
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");

                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<ActionResult> CreatePost(string title, string description, HttpPostedFileBase image)
        {
            try
            {
                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description))
                {
                    TempData["Error"] = "All fields are required.";
                    return RedirectToAction("Index", "Home");
                }

                // Dodajte debug liniju za proveru userId-a
                var loggedInUser = (User)Session["LoggedInUser"];
                // Upload image if provided


                var newPost = new Post()
                {
                    Title = title,
                    Description = description,
                    PostedByUser = loggedInUser.Username,
                    //ImageUrl = imageUrl,
                    UserId = Guid.Parse(loggedInUser.RowKey),
                    CreatedAt = DateTime.UtcNow.AddHours(2)
                };

                string imageUrl = null;
                if (image != null && image.ContentLength > 0)
                {
                    using (var img = Image.FromStream(image.InputStream))
                    {
                        var blobService = new PostImageBlob();
                        string blobName = "post" + newPost.RowKey; // Generate a unique name for the image
                        imageUrl = blobService.UploadImage(img, "post-images", blobName);
                    }
                }
                newPost.ImageUrl = imageUrl;

                await postDataRepository.CreatePost(newPost);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating post: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }




        [HttpPost]
        public async Task<ActionResult> AddComment(string postId, string commentText)
        {
            try
            {
                if (string.IsNullOrEmpty(postId) || string.IsNullOrEmpty(commentText))
                {
                    TempData["Error"] = "Invalid comment data.";
                    return RedirectToAction("Index", "Home");
                }

                if (Session["LoggedInUser"] == null)
                {
                    TempData["Error"] = "You must be logged in to add a comment.";
                    return RedirectToAction("Index", "Home");
                }

                User loggedUser = (User)Session["LoggedInUser"];

                var newComment = new Comment
                {
                    PartitionKey = "Comment",
                    PostID = postId,
                    UserID = Guid.Parse(loggedUser.RowKey),
                    Username = loggedUser.Username,
                    CreatedAt = DateTime.UtcNow.AddHours(2),
                    NumberOfDislikes = 0,
                    NumberOfLikes = 0,
                    IsDeleted = false,
                    Likes = new List<string>(),
                    Dislikes = new List<string>(),
                    Description = commentText
                };

                await commentDataRepository.CreateComment(newComment); // Await the async method
                await postDataRepository.IncrementCommentCount(postId); // Increment the comment count

                CloudQueue queue = CommentQueue.GetQueueReference("notifications");
                queue.AddMessage(new CloudQueueMessage(newComment.RowKey), null, TimeSpan.FromMilliseconds(30));


                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error adding comment: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }


        [HttpPost]
        public async Task<JsonResult> LikePost(string postId, string username)
        {
            try
            {
                Guid postGuid = Guid.Parse(postId);
                User user = await userDataRepository.GetUserByUsername(username);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }
                Guid userGuid = Guid.Parse(user.RowKey);

                var reaction = await postReactionsDataRepository.GetPostReactionAsync(postGuid, userGuid);

                if (reaction == null)
                {
                    reaction = new PostReactions
                    {
                        PostId = postGuid,
                        UserId = userGuid,
                        Liked = true,
                        Disliked = false
                    };
                    await postReactionsDataRepository.InsertOrUpdatePostReactionAsync(reaction);
                    await postDataRepository.IncrementLikes(postId);
                }
                else if (reaction.Liked)
                {
                    reaction.Liked = false;
                    await postReactionsDataRepository.InsertOrUpdatePostReactionAsync(reaction);
                    await postDataRepository.DecrementLikes(postId);
                }
                else
                {
                    reaction.Liked = true;
                    if (reaction.Disliked)
                    {
                        reaction.Disliked = false;
                        await postDataRepository.DecrementDislikes(postId);
                    }
                    await postReactionsDataRepository.InsertOrUpdatePostReactionAsync(reaction);
                    await postDataRepository.IncrementLikes(postId);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LikePost: {ex.Message}");
                return Json(new { success = false, message = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> DislikePost(string postId, string username)
        {
            try
            {
                Guid postGuid = Guid.Parse(postId);
                User user = await userDataRepository.GetUserByUsername(username);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }
                Guid userGuid = Guid.Parse(user.RowKey);

                var reaction = await postReactionsDataRepository.GetPostReactionAsync(postGuid, userGuid);

                if (reaction == null)
                {
                    reaction = new PostReactions
                    {
                        PostId = postGuid,
                        UserId = userGuid,
                        Liked = false,
                        Disliked = true
                    };
                    await postReactionsDataRepository.InsertOrUpdatePostReactionAsync(reaction);
                    await postDataRepository.IncrementDislikes(postId);
                }
                else if (reaction.Disliked)
                {
                    reaction.Disliked = false;
                    await postReactionsDataRepository.InsertOrUpdatePostReactionAsync(reaction);
                    await postDataRepository.DecrementDislikes(postId);
                }
                else
                {
                    reaction.Disliked = true;
                    if (reaction.Liked)
                    {
                        reaction.Liked = false;
                        await postDataRepository.DecrementLikes(postId);
                    }
                    await postReactionsDataRepository.InsertOrUpdatePostReactionAsync(reaction);
                    await postDataRepository.IncrementDislikes(postId);
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DislikePost: {ex.Message}");
                return Json(new { success = false, message = "Internal server error" });
            }
        }



        [HttpPost]
        public async Task<JsonResult> LikeComment(string commentId, string username)
        {
            try
            {
                Guid commentGuid = Guid.Parse(commentId);
                User user = await userDataRepository.GetUserByUsername(username);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                Guid userGuid = Guid.Parse(user.RowKey);
                var reaction = await commentReactionsDataRepository.GetCommentReactionAsync(commentGuid, userGuid);
                bool newLikeStatus = false;

                if (reaction == null)
                {
                    reaction = new CommentReactions
                    {
                        CommentId = commentGuid,
                        UserId = userGuid,
                        Liked = true,
                        Disliked = false
                    };
                    await commentReactionsDataRepository.InsertOrUpdateCommentReactionAsync(reaction);
                    newLikeStatus = true;
                }
                else if (reaction.Liked)
                {
                    reaction.Liked = false;
                    await commentReactionsDataRepository.InsertOrUpdateCommentReactionAsync(reaction);
                    await commentDataRepository.DecrementLikes(commentId);
                }
                else
                {
                    reaction.Liked = true;
                    if (reaction.Disliked)
                    {
                        reaction.Disliked = false;
                        await commentDataRepository.DecrementDislikes(commentId);
                    }
                    await commentReactionsDataRepository.InsertOrUpdateCommentReactionAsync(reaction);
                    newLikeStatus = true;
                }

                if (newLikeStatus)
                {
                    await commentDataRepository.IncrementLikes(commentId);
                }

                return Json(new { success = true });
            }
            catch (FormatException fe)
            {
                System.Diagnostics.Debug.WriteLine($"Format error in LikeComment: {fe.Message}");
                return Json(new { success = false, message = "Invalid comment or user ID format" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LikeComment: {ex.Message}");
                return Json(new { success = false, message = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> DislikeComment(string commentId, string username)
        {
            try
            {
                Guid commentGuid = Guid.Parse(commentId);
                User user = await userDataRepository.GetUserByUsername(username);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                Guid userGuid = Guid.Parse(user.RowKey);
                var reaction = await commentReactionsDataRepository.GetCommentReactionAsync(commentGuid, userGuid);
                bool newDislikeStatus = false;

                if (reaction == null)
                {
                    reaction = new CommentReactions
                    {
                        CommentId = commentGuid,
                        UserId = userGuid,
                        Liked = false,
                        Disliked = true
                    };
                    await commentReactionsDataRepository.InsertOrUpdateCommentReactionAsync(reaction);
                    newDislikeStatus = true;
                }
                else if (reaction.Disliked)
                {
                    reaction.Disliked = false;
                    await commentReactionsDataRepository.InsertOrUpdateCommentReactionAsync(reaction);
                    await commentDataRepository.DecrementDislikes(commentId);
                }
                else
                {
                    reaction.Disliked = true;
                    if (reaction.Liked)
                    {
                        reaction.Liked = false;
                        await commentDataRepository.DecrementLikes(commentId);
                    }
                    await commentReactionsDataRepository.InsertOrUpdateCommentReactionAsync(reaction);
                    newDislikeStatus = true;
                }

                if (newDislikeStatus)
                {
                    await commentDataRepository.IncrementDislikes(commentId);
                }

                return Json(new { success = true });
            }
            catch (FormatException fe)
            {
                System.Diagnostics.Debug.WriteLine($"Format error in DislikeComment: {fe.Message}");
                return Json(new { success = false, message = "Invalid comment or user ID format" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in DislikeComment: {ex.Message}");
                return Json(new { success = false, message = "Internal server error" });
            }
        }





        [HttpPost]
        public async Task<JsonResult> DeletePost(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "Invalid post ID." });
            }

            try
            {
                var loggedInUserId = Session["LoggedInUserId"] as string;
                if (string.IsNullOrEmpty(loggedInUserId))
                {
                    return Json(new { success = false, message = "User not logged in." });
                }

                await postDataRepository.DeletePost(id, loggedInUserId);
                return Json(new { success = true, message = "Post successfully deleted." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while deleting the post: " + ex.Message });
            }
        }

    }
}