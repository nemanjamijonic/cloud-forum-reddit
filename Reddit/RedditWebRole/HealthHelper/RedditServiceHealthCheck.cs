using Common.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RedditWebRole.HealthHelper
{
    public class RedditServiceHealthCheck : IRedditHealth
    {
        public void IsRedditServiceAlive()
        {
            Console.WriteLine("Health Monitoring has poked Reddit Service");
        }
    }
}