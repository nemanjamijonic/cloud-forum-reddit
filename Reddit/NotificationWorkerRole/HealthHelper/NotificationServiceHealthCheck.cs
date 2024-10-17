using Common.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationWorkerRole.HealthHelper
{
    public class NotificationServiceHealthCheck : INotificationHealth
    {
        public void IsNotificationServiceAlive()
        {
            Console.WriteLine("Health Monitoring has poked Notification Service");
        }
    }
}
