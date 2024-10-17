using Common.Contracts;
using Common.Models;
using Common.Queues;
using Common.Repositories;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Diagnostics;
using System.Net;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace HealthMonitoringWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private ChannelFactory<INotificationHealth> notificationServiceFactory;
        private INotificationHealth notificationServiceClient;

        private ChannelFactory<IRedditHealth> redditServiceFactory;
        private IRedditHealth redditServiceClient;

        private HealthCheckDataRepository healthCheckDataRepository;
        private CloudQueue adminQueue;

        public override void Run()
        {
            Trace.TraceInformation("HealthMonitoringWorkerRole is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 12;

            // Initialize the service clients and repository
            InitializeServiceClients();
            healthCheckDataRepository = new HealthCheckDataRepository();

            // Initialize the queue
            adminQueue = AdminNotificationQueue.GetQueueReference("adminnotifications");

            bool result = base.OnStart();

            Trace.TraceInformation("HealthMonitoringWorkerRole has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("HealthMonitoringWorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            // Close the service clients
            CloseServiceClients();

            base.OnStop();

            Trace.TraceInformation("HealthMonitoringWorkerRole has stopped");
        }

        private void InitializeServiceClients()
        {
            NetTcpBinding binding = new NetTcpBinding();
                                                                                                                                  //6000 je dobar port
            notificationServiceFactory = new ChannelFactory<INotificationHealth>(binding, new EndpointAddress("net.tcp://localhost:6000/NotificationHealthCheck"));
            notificationServiceClient = notificationServiceFactory.CreateChannel();
                                                                                                                      //6001 je dobar port
            redditServiceFactory = new ChannelFactory<IRedditHealth>(binding, new EndpointAddress("net.tcp://localhost:6001/RedditHealthCheck"));
            redditServiceClient = redditServiceFactory.CreateChannel();
        }

        private void CloseServiceClients()
        {
            if (notificationServiceFactory != null)
            {
                try
                {
                    notificationServiceFactory.Close();
                }
                catch (CommunicationException)
                {
                    notificationServiceFactory.Abort();
                }
            }

            if (redditServiceFactory != null)
            {
                try
                {
                    redditServiceFactory.Close();
                }
                catch (CommunicationException)
                {
                    redditServiceFactory.Abort();
                }
            }
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Trace.TraceInformation("Performing health checks...");

                await PerformHealthCheck("NotificationService", notificationServiceClient.IsNotificationServiceAlive);
                await PerformHealthCheck("RedditService", redditServiceClient.IsRedditServiceAlive);

                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
        }

        private async Task PerformHealthCheck(string serviceName, Action healthCheckAction)
        {
            HealthCheck healthCheck = new HealthCheck
            {
                CheckedAt = DateTime.UtcNow.AddHours(2),
                ServiceUrl = serviceName
            };

            try
            {
                healthCheckAction();
                healthCheck.Status = "OK";
                Trace.TraceInformation($"{serviceName} is alive.");
            }
            catch (Exception ex)
            {
                healthCheck.Status = "NOT_OK";
                adminQueue.AddMessage(new CloudQueueMessage(serviceName),null,TimeSpan.FromMilliseconds(30));
                Trace.TraceError($"An error occurred during health check for {serviceName}: {ex.Message}");
            }

            await healthCheckDataRepository.LogHealthCheck(healthCheck);
        }
    }
}
