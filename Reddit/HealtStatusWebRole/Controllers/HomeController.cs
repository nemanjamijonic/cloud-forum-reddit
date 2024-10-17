using Common.Models;
using Common.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HealtStatusWebRole.Controllers
{
    public class HomeController : Controller
    {
        private HealthCheckDataRepository _repository = new HealthCheckDataRepository();

        public async Task<ActionResult> Index()
        {
            DateTime lastHour = DateTime.UtcNow;
            DateTime last24Hours = DateTime.UtcNow.AddHours(-22);

            var redditServiceLastHour = await _repository.GetRedditServiceHealthChecksAsync(lastHour);
            var redditServiceLast24Hours = await _repository.GetRedditServiceHealthChecksAsync(last24Hours);

            var notificationServiceLastHour = await _repository.GetNotificationServiceHealthChecksAsync(lastHour);
            var notificationServiceLast24Hours = await _repository.GetNotificationServiceHealthChecksAsync(last24Hours);

            ViewBag.RedditServiceLastHourUptime = CalculateUptimePercentage(redditServiceLastHour);
            ViewBag.RedditServiceLast24HoursUptime = CalculateUptimePercentage(redditServiceLast24Hours);

            // Sačuvajte samo 80 najnovijih zapisa za Reddit Service
            ViewBag.RedditServiceLast24HoursChecks = redditServiceLast24Hours
                .Take(80)
                .ToList();

            ViewBag.NotificationServiceLastHourUptime = CalculateUptimePercentage(notificationServiceLastHour);
            ViewBag.NotificationServiceLast24HoursUptime = CalculateUptimePercentage(notificationServiceLast24Hours);

            // Sačuvajte samo 80 najnovijih zapisa za Notification Service
            ViewBag.NotificationServiceLast24HoursChecks = notificationServiceLast24Hours
                .Take(80)
                .ToList();

            return View();
        }





        private double CalculateUptimePercentage(IEnumerable<HealthCheck> checks)
        {
            if (!checks.Any()) return 0;

            int totalChecks = checks.Count();
            int okChecks = checks.Count(c => c.Status == "OK");

            return (double)okChecks / totalChecks * 100;
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