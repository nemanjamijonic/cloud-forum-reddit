using System;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Common.Email
{
    public class EmailService
    {
        public EmailService()
        {
        }

        public async Task SendEmail(string toEmail, string userData, string subject, string plainTextContent, string htmlContent)
        {
            const int maxRetries = 3;
            int attempt = 0;

            while (attempt < maxRetries)
            {
                try
                {
                    string smtpServer = "smtp.gmail.com";
                    int port = 587;

                    // Adresa i lozinka za autentifikaciju na SMTP serveru
                    string username = "forumdrs2023@gmail.com";
                    string password = "wtez cskt ddtm uqbx";
                    string fromEmail = "forumdrs2023@gmail.com";

                    using (var client = new SmtpClient(smtpServer, port))
                    {
                        client.EnableSsl = true;
                        client.Credentials = new NetworkCredential(username, password);

                        using (var message = new MailMessage(fromEmail, toEmail))
                        {
                            message.Subject = subject;
                            message.Body = htmlContent;
                            message.IsBodyHtml = true;
                            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(plainTextContent, null, "text/plain"));
                            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlContent, null, "text/html"));

                            await client.SendMailAsync(message);
                            Trace.TraceInformation("Email sent successfully.");
                            return; // Email sent successfully, exit the loop
                        }
                    }
                }
                catch (Exception ex)
                {
                    attempt++;
                    Trace.TraceError($"Attempt {attempt} - Error sending email: {ex.Message}\nStack Trace: {ex.StackTrace}");

                    if (attempt == maxRetries)
                    {
                        throw; // Re-throw the exception after the last attempt
                    }

                    // Optional: Add a delay before retrying
                    await Task.Delay(TimeSpan.FromSeconds(2 * attempt));
                }
            }
        }
    }
}