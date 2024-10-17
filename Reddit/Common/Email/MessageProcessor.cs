using Common.Email;
using Common.Models;
using Common.Queues;
using Common.Repositories;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class MessageProcessor
{
    private readonly NotificationDataRepository notificationDataRepository;
    private readonly UserDataRepository userDataRepository;
    private readonly PostDataRepository postDataRepository;
    private readonly CommentDataRepository commentDataRepository;
    private readonly EmailService emailService;

    public MessageProcessor(
        NotificationDataRepository notificationDataRepository,
        UserDataRepository userDataRepository,
        PostDataRepository postDataRepository,
        CommentDataRepository commentDataRepository,
        EmailService emailService)
    {
        this.notificationDataRepository = notificationDataRepository;
        this.userDataRepository = userDataRepository;
        this.postDataRepository = postDataRepository;
        this.commentDataRepository = commentDataRepository;
        this.emailService = emailService;
    }

    private static List<string> LoadEmails()
    {
        var adminEmails = new List<string>();
        var relativeFilePath = @"..\..\..\..\..\..\AdminsEmailConfigurator\bin\Debug\AdminEmails.txt";
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var filePath = Path.GetFullPath(Path.Combine(baseDirectory, relativeFilePath));

        Trace.TraceInformation($"Base directory: {baseDirectory}");
        Trace.TraceInformation($"Calculated file path: {filePath}");

        if (File.Exists(filePath))
        {
            adminEmails.AddRange(File.ReadAllLines(filePath));
        }
        else
        {
            Trace.TraceWarning($"Admin email file not found: {filePath}");
        }

        return adminEmails;
    }

    private string GenerateAdminContactsHtml(List<string> adminEmails)
    {
        var adminContacts = string.Join("<br />", adminEmails.Select(email => $"<a href='mailto:{email}'>{email}</a>"));
        return adminContacts;
    }

    public async Task<int> ProcessMessageAsync(CloudQueueMessage message, CloudQueue queue)
    {
        try
        {
            var adminEmails = LoadEmails();
            var adminContactsHtml = GenerateAdminContactsHtml(adminEmails);
            var postId = await commentDataRepository.GetPostIdByCommentId(message.AsString);
            var notifications = notificationDataRepository.GetNotificationsForPost(Guid.Parse(postId));
            var comment = await commentDataRepository.GetComment(message.AsString);
            var post = await postDataRepository.GetPostById(postId);
            int sentMailsCount = 0;
            foreach (var notification in notifications)
            {
                User user = await userDataRepository.GetUserById(notification.UserId.ToString());
                var userEmail = user?.Email;

                if (!string.IsNullOrEmpty(userEmail))
                {
                    string subject = $"Reddit Forum Notification: New Comment on '{post.Title}'";
                    var userData = $"{user.FirstName} {user.LastName}";
                    string plainTextContent = $"Dear {userData},\n\nYou have a new comment on the post you're subscribed to:\n\n" +
                                              $"Post Title: {post.Title}\n" +
                                              $"Post Description: {post.Description}\n" +
                                              $"Comment by {comment.Username}: {comment.Description}\n\n" +
                                              "This is an automated message. Please do not reply.\n" +
                                              $"If you have any questions, contact our administrators at:\n{string.Join("\n", adminEmails)}.";
                    string htmlContent = $"<p><b>Dear {userData},</b></p>" +
                                         "<p>You have a new comment on the post you're subscribed to:</p>" +
                                         $"<p><strong>Post Title:</strong> {post.Title}</p>" +
                                         $"<p><strong>Post Description:</strong> {post.Description}</p>" +
                                         $"<p><strong>Comment by {comment.Username}:</strong></p>" +
                                         $"<p><strong>Comment description:</strong> {comment.Description}</p>" +
                                         "<p>This is an automated message sent by <b>Cloud Reddit Forum Notification Service</b>. Please do not reply.</p>" +
                                         $"<p>If you have any questions, contact our administrators:<br />{adminContactsHtml}</p>";

                    await emailService.SendEmail(userEmail, userData, subject, plainTextContent, htmlContent);
                    sentMailsCount++;
                }
            }

            // If message processing exceeds 3 attempts, delete it from the queue
            if (message.DequeueCount > 3)
            {
                await queue.DeleteMessageAsync(message);
                Trace.TraceInformation($"Message deleted after exceeding max dequeue count: {message.AsString}");
            }

            Trace.TraceInformation($"Message processed: {message.AsString}", "Information");

            return sentMailsCount; // Return the number of sent emails
        }
        catch (Exception ex)
        {
            Trace.TraceError($"Error processing message: {ex.Message}\nStack Trace: {ex.StackTrace}");
            return 0; // Indicate processing failure by returning 0
        }
    }
}
