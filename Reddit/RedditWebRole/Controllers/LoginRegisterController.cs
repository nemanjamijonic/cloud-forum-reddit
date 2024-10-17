using Common.Blobs;
using Common.Enums;
using Common.Models;
using Common.Repositories;
using RedditWebRole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using Common.Helper;

namespace RedditWebRole.Controllers
{

    public class LoginRegisterController : Controller
    {

        UserDataRepository repo = new UserDataRepository();
        PostDataRepository postRepo = new PostDataRepository();
        // GET: LoginRegister
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }



        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(string email, string password)
        {
            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                var hasedPassord = HashHelper.ConvertToHash(password);
                User user = await repo.GetUserByUsernameAndPassword(email, hasedPassord);
                if (user != null)
                {
                    Session["LoggedInUser"] = user;
                    Session["LoggedInUserId"] = user.RowKey; // Store user ID in session
                    Session["PostFromLoggedInUser"] = postRepo.RetrieveAllPostsForUser(user.RowKey);

                    // Set User Image URL
                    ViewBag.UserImageUrl = user.ImageUrl;
                    Console.WriteLine("UserImageUrl: " + user.ImageUrl); // Debugging output

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.ErrorMessage = "Pogrešno korisničko ime ili lozinka.";
                    return View("Login");
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Molimo unesite korisničko ime i lozinku.";
                return View("Login");
            }
        }



        [HttpPost]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            User existingUser = await repo.GetUserByUsername(model.Username);
            if (existingUser != null)
            {
                ViewBag.ErrorMessage = "Korisničko ime (email) već postoji. Molimo izaberite drugo korisničko ime (email).";
                return View(model);
            }

            User user = new User
            {
                PartitionKey = "User",
                Username = model.Username,
                Password = HashHelper.ConvertToHash(model.Password),
                FirstName = model.FirstName,
                LastName = model.LastName,
                Address = model.Address,
                City = model.City,
                Country = model.Country,
                PhoneNumber = model.PhoneNumber,
                LikedPosts = new List<string>(),
                DislikedPosts = new List<string>(),
                Email = model.Email,
            };

            string imageUrl = null;
            if (model.ImageFile != null && model.ImageFile.ContentLength > 0)
            {
                using (var img = Image.FromStream(model.ImageFile.InputStream))
                {
                    var blobService = new UserImageBlob();
                    imageUrl = blobService.UploadImage(img, "user-images", "user" + user.RowKey);
                }
            }

            user.ImageUrl = imageUrl;
            repo.CreateUser(user);

            Session["LoggedInUser"] = user;
            Session["LoggedInUserId"] = user.RowKey; // Store user ID in session

            // Set User Image URL
            ViewBag.UserImageUrl = user.ImageUrl;
            Console.WriteLine("UserImageUrl: " + user.ImageUrl); // Debugging output

            return RedirectToAction("Index", "Home");
        }


    }
}