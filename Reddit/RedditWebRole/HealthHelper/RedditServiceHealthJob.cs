using Common.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Web;

namespace RedditWebRole.HealthHelper
{
    public class RedditServiceHealthJob
    {
        public ServiceHost ServiceHost;

        public RedditServiceHealthJob()
        {
            Start();
        }

        public void Start()
        {
            ServiceHost = new ServiceHost(typeof(RedditServiceHealthCheck));
            NetTcpBinding binding = new NetTcpBinding();
            ServiceHost.AddServiceEndpoint(typeof(IRedditHealth), binding, new Uri("net.tcp://localhost:6001/RedditHealthCheck"));
            ServiceHost.Open();
            Console.WriteLine("Reddit Service Health Check is ready.");
        }
    }
}