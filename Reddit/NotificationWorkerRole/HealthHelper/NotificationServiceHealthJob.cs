using Common.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace NotificationWorkerRole.HealthHelper
{
    public class NotificationServiceHealthJob
    {
        public ServiceHost ServiceHost;

        public NotificationServiceHealthJob()
        {
            Start();
        }

        public void Start()
        {
            ServiceHost = new ServiceHost(typeof(NotificationServiceHealthCheck));
            NetTcpBinding binding = new NetTcpBinding();
            ServiceHost.AddServiceEndpoint(typeof(INotificationHealth), binding, new Uri("net.tcp://localhost:6000/NotificationHealthCheck"));
            ServiceHost.Open();
            Console.WriteLine("Notification Service Health Check is ready.");
        }
    }
}
