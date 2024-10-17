using Common.Blobs;
using Common.Models;
using Common.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RedditWebRole.Controllers
{
    public class HomeController : Controller
    {
        PostDataRepository postDataRepo = new PostDataRepository();
        CommentDataRepository commentDataRepository = new CommentDataRepository();
        UserImageBlob userBlob = new UserImageBlob();

        public ActionResult Index()
        {
            User loggedInUser = Session["LoggedInUser"] as User;
            if (loggedInUser != null)
            {
                ViewBag.LoggedInUsername = loggedInUser.Username;
                ViewBag.UserImageUrl = loggedInUser.ImageUrl;
                ViewBag.UserImage = userBlob.DownloadImage("user-images", "user" + loggedInUser.RowKey);

                // Pretpostavljamo da User ima svojstva LikedPosts i DislikedPosts
                ViewBag.LikedPosts = loggedInUser.LikedPosts ?? new List<string>();
                ViewBag.DislikedPosts = loggedInUser.DislikedPosts ?? new List<string>();
                Session["PostFromLoggedInUser"] = postDataRepo.RetrieveAllPostsForUser(loggedInUser.RowKey);
            }
            else
            {
                ViewBag.LikedPosts = new List<string>();
                ViewBag.DislikedPosts = new List<string>();
            }

            // Preuzmi postove iz TempData ako su dostupni
            var posts = TempData["FilteredSortedPosts"] as List<Post>;
            if (posts == null)
            {
                // Učitaj sve postove ako nema filtera/sortiranja
                posts = postDataRepo.RetrieveAllPosts().ToList();
            }

            // Učitaj komentare za svaki post
            List<Post> postoviSaKomentarima = new List<Post>();
            foreach (var post in posts)
            {
                post.Comments = commentDataRepository.RetrieveCommentsForPost(post.RowKey).ToList();
                post.CommentsNumber = post.Comments.Count();
                postoviSaKomentarima.Add(post);
            }

            return View(postoviSaKomentarima);
        }




        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

    }
}