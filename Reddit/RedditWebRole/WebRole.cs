using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using RedditWebRole.HealthHelper;
using System.Diagnostics;
using System.ServiceModel;

namespace RedditWebRole
{
    public class WebRole : RoleEntryPoint
    {
        private RedditServiceHealthJob redditServiceHealthJob;

        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at https://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();
            redditServiceHealthJob = new RedditServiceHealthJob();
            Trace.TraceInformation("RedditWebRole has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("RedditWebRole is stopping");

            // Close the RedditServiceHealthJob
            if (redditServiceHealthJob != null && redditServiceHealthJob.ServiceHost != null)
            {
                try
                {
                    redditServiceHealthJob.ServiceHost.Close();
                }
                catch (CommunicationException e)
                {
                    Trace.TraceError($"An error occurred while closing the service host: {e.Message}", "Error");
                    redditServiceHealthJob.ServiceHost.Abort();
                }
            }

            base.OnStop();
            Trace.TraceInformation("RedditWebRole has stopped");
        }
    }
}
