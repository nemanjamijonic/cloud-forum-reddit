using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Common.Models;
using Common.Repositories;

namespace RedditWebRole.Controllers
{
    public class NotificationController : Controller
    {
        private readonly NotificationDataRepository _notificationDataRepo = new NotificationDataRepository();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ManageNotifications(Guid userId, Guid postId)
        {
            try
            {
                // Proveravamo da li notifikacija već postoji
                var notification = await _notificationDataRepo.GetNotification(userId, postId);

                if (notification != null)
                {
                    // Ako notifikacija postoji, menjamo IsNotificationOn na suprotno
                    notification.IsNotificationOn = !notification.IsNotificationOn;
                    await _notificationDataRepo.UpdatePost(notification);
                }
                else
                {
                    // Ako ne postoji, kreiramo novu notifikaciju sa IsNotificationOn postavljenim na true
                    var newNotification = new Notification
                    {
                        PartitionKey = "Notification",
                        RowKey = Guid.NewGuid().ToString(),
                        UserId = userId,
                        PostId = postId,
                        IsNotificationOn = true
                    };
                    await _notificationDataRepo.CreatePost(newNotification);
                }

                // Vraćanje uspešnog odgovora
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Logovanje greške
                System.Diagnostics.Debug.WriteLine($"Error in ManageNotifications: {ex.Message}");
                return new HttpStatusCodeResult(500, "Internal server error");
            }
        }


    }
}
