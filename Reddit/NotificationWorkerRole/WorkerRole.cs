using Common.Email;
using Common.Models;
using Common.Queues;
using Common.Repositories;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage.Queue;
using NotificationWorkerRole.HealthHelper; // Include this namespace to access NotificationServiceHealthJob
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

public class WorkerRole : RoleEntryPoint
{
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
    private readonly NotificationDataRepository notificationDataRepository = new NotificationDataRepository();
    private readonly UserDataRepository userDataRepository = new UserDataRepository();
    private readonly PostDataRepository postDataRepository = new PostDataRepository();
    private readonly CommentDataRepository commentDataRepository = new CommentDataRepository();
    private readonly SentMailsDataRepository sentMailsDataRepository = new SentMailsDataRepository();
    private MessageProcessor messageProcessor;
    private EmailService emailService;
    private CloudQueue adminQueue;
    private NotificationServiceHealthJob notificationServiceHealthJob;

    private List<string> adminEmails;
    private readonly Dictionary<string, DateTime> lastEmailSentTimes = new Dictionary<string, DateTime>();


    public override void Run()
    {
        CloudQueue queue = CommentQueue.GetQueueReference("notifications");
        adminQueue = AdminNotificationQueue.GetQueueReference("adminnotifications");
        Trace.TraceInformation("Notification_WorkerRole entry point called", "Information");

        emailService = new EmailService(); // Initialize emailService
        messageProcessor = new MessageProcessor(
            notificationDataRepository,
            userDataRepository,
            postDataRepository,
            commentDataRepository,
            emailService);

        // Start processing messages in a separate async method to use async/await properly
        RunAsync(cancellationTokenSource.Token).GetAwaiter().GetResult();
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        CloudQueue queue = CommentQueue.GetQueueReference("notifications");
        adminQueue = AdminNotificationQueue.GetQueueReference("adminnotifications");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                CloudQueueMessage message = await queue.GetMessageAsync();
                if (message == null)
                {
                    Trace.TraceInformation("Trenutno ne postoji poruka u redu.", "Information");
                }
                else
                {
                    Trace.TraceInformation($"Poruka glasi: {message.AsString}", "Information");

                    var result = await messageProcessor.ProcessMessageAsync(message, queue);

                    if (result != 0)
                    {
                        SentMail sentMail = new SentMail()
                        {
                            NumberOfSentMails = result,
                            SentAt = DateTime.UtcNow.AddHours(2),
                            CommentID = message.AsString
                        };
                        await sentMailsDataRepository.CreateNewNotificationGroup(sentMail);
                        await queue.DeleteMessageAsync(message);
                        Trace.TraceInformation("Message deleted successfully.", "Information");
                    }
                    else
                    {
                        Trace.TraceWarning("Message processing failed. It will not be deleted and will be retried.", "Warning");
                    }

                    await Task.Delay(5000);
                    Trace.TraceInformation("Working", "Information");
                }

                CloudQueueMessage adminMessage = await adminQueue.GetMessageAsync();
                if (adminMessage == null)
                {
                    Trace.TraceInformation("No messages in admin notifications queue.", "Information");
                }
                else
                {
                    Trace.TraceInformation($"Message received: {adminMessage.AsString}", "Information");

                    await SendFailureNotification(adminMessage.AsString);
                    await adminQueue.DeleteMessageAsync(adminMessage);

                    Trace.TraceInformation("Message deleted successfully.", "Information");

                    await Task.Delay(5000);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"An error occurred: {ex.Message}", "Error");
            }
        }
    }




    public override bool OnStart()
    {
        ServicePointManager.DefaultConnectionLimit = 12;

        notificationServiceHealthJob = new NotificationServiceHealthJob();
        adminEmails = LoadAdminEmails.LoadEmails();

        bool result = base.OnStart();

        Trace.TraceInformation("NotificationWorkerRole has been started");

        return result;
    }

    public override void OnStop()
    {
        Trace.TraceInformation("NotificationWorkerRole is stopping");

        cancellationTokenSource.Cancel();
        runCompleteEvent.WaitOne();

        if (notificationServiceHealthJob?.ServiceHost != null)
        {
            try
            {
                notificationServiceHealthJob.ServiceHost.Close();
            }
            catch (CommunicationException e)
            {
                Trace.TraceError($"An error occurred while closing the service host: {e.Message}", "Error");
                notificationServiceHealthJob.ServiceHost.Abort();
            }
        }

        base.OnStop();

        Trace.TraceInformation("NotificationWorkerRole has stopped");
    }


    private async Task SendFailureNotification(string serviceName)
    {
        if (emailService == null)
        {
            Trace.TraceError("EmailService is null", "Error");
            return;
        }

        if (adminEmails == null || adminEmails.Count == 0)
        {
            Trace.TraceError("Admin emails list is null or empty", "Error");
            return;
        }

        if (sentMailsDataRepository == null)
        {
            Trace.TraceError("SentMailsDataRepository is null", "Error");
            return;
        }

        DateTime currentTime = DateTime.UtcNow;
        if (lastEmailSentTimes.TryGetValue(serviceName, out DateTime lastSentTime))
        {
            if (currentTime < lastSentTime.AddMinutes(30))
            {
                Trace.TraceInformation($"Email already sent for {serviceName} within the last 30 minutes. Skipping email.");
                return;
            }
        }

        lastEmailSentTimes[serviceName] = currentTime;

        string subject = $"Health Check Failure for {serviceName}";
        string body = $"<br /><br /><p>The health check for <b>{serviceName}</b> failed with the error!!!</p>" +
            $"<p> Take a closer look in order to fix the issue. </p>" +
            $"<p>This is an automated message. Please do not reply.</p>";

        foreach (var toEmailAddress in adminEmails)
        {
            await emailService.SendEmail(toEmailAddress, "", subject, "", body);
        }

        SentMail sentMail = new SentMail
        {
            NumberOfSentMails = adminEmails.Count,
            SentAt = DateTime.UtcNow.AddHours(2),
            CommentID = serviceName
        };

        await sentMailsDataRepository.CreateNewNotificationGroup(sentMail);
    }

}
