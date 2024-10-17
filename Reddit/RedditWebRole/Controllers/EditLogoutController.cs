using Common.Blobs;
using Common.Models;
using Common.Repositories;
using RedditWebRole.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RedditWebRole.Controllers
{

    public class EditLogoutController : Controller
    {
        UserDataRepository repo = new UserDataRepository();
        // GET: EditLogout
        public ActionResult Index()
        {
            User loggedInUser = Session["LoggedInUser"] as User;
            ViewBag.UserImageUrl = loggedInUser.ImageUrl;
            return View();
        }

        public ActionResult EditUser()
        {
            if (Session["LoggedInUser"] != null)
            {
                var loggedInUser = (User)Session["LoggedInUser"];

                if (loggedInUser != null)
                {
                    EditViewModel model = new EditViewModel
                    {
                        FirstName = loggedInUser.FirstName,
                        LastName = loggedInUser.LastName,
                        Address = loggedInUser.Address,
                        City = loggedInUser.City,
                        Country = loggedInUser.Country,
                        PhoneNumber = loggedInUser.PhoneNumber,
                        Email = loggedInUser.Email,
                        ImageUrl = loggedInUser.ImageUrl
                    };

                    ViewBag.UserImageUrl = loggedInUser.ImageUrl; // Set the image URL in ViewBag

                    return View(model);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<ActionResult> EditUser(EditViewModel model, HttpPostedFileBase ImageUrl)
        {
            if (ModelState.IsValid)
            {
                var loggedInUser = (User)Session["LoggedInUser"];
                if (loggedInUser != null)
                {
                    loggedInUser.FirstName = model.FirstName;
                    loggedInUser.LastName = model.LastName;
                    loggedInUser.Address = model.Address;
                    loggedInUser.City = model.City;
                    loggedInUser.Country = model.Country;
                    loggedInUser.PhoneNumber = model.PhoneNumber;

                    var blobService = new UserImageBlob();

                    if (ImageUrl != null && ImageUrl.ContentLength > 0)
                    {
                        using (var img = Image.FromStream(ImageUrl.InputStream))
                        {
                            string blobName = "user" + loggedInUser.RowKey;
                            string imageUrl = blobService.UploadImage(img, "user-images", blobName);
                            loggedInUser.ImageUrl = $"{imageUrl}?timestamp={DateTime.UtcNow.Ticks}";
                        }
                    }

                    await repo.UpdateUser(loggedInUser);
                    Session["LoggedInUser"] = loggedInUser;
                    ViewBag.UserImageUrl = loggedInUser.ImageUrl; // Update ViewBag with new ImageUrl

                    return RedirectToAction("Index", "Home");
                }
            }
            return View(model);
        }





        public ActionResult Logout()
        {
            Session["LoggedInUser"] = null;
            return RedirectToAction("Index", "Home");
        }
    }
}