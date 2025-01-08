namespace ETicketApp.Services
{
     using Microsoft.Extensions.Configuration;
     using Microsoft.AspNetCore.Identity.UI.Services;
     using System.Net;
     using System.Net.Mail;
     using System.Threading.Tasks;
     using System;
     using Microsoft.Extensions.Logging;

     public class EmailSender : IEmailSender
     {
          private readonly IConfiguration _configuration;
          private readonly ILogger<EmailSender> _logger;

          public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
          {
               _configuration = configuration;
               _logger = logger;
          }

          public async Task SendEmailAsync(string email, string subject, string message)
          {
               try
               {
                    // Ensure the SMTP values are not null or empty
                    string smtpServer = _configuration["EmailSettings:SmtpServer"];
                    string smtpPort = _configuration["EmailSettings:SmtpPort"];
                    string smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                    string smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                    string fromEmail = _configuration["EmailSettings:FromEmail"];

                    if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPort) ||
                        string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword) ||
                        string.IsNullOrEmpty(fromEmail))
                    {
                         throw new InvalidOperationException("SMTP settings are not properly configured.");
                    }

                    _logger.LogInformation("SMTP configuration is valid. Attempting to send email...");

                    var smtpClient = new SmtpClient(smtpServer)
                    {
                         Port = int.Parse(smtpPort),
                         Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                         EnableSsl = true
                    };

                    _logger.LogInformation("SMTP client configured, now setting up the email message...");

                    var mailMessage = new MailMessage
                    {
                         From = new MailAddress(fromEmail),
                         Subject = subject,
                         Body = message,
                         IsBodyHtml = true,
                    };
                    mailMessage.To.Add(email);

                    _logger.LogInformation($"Sending email to {email} with subject '{subject}'...");

                    // Send email asynchronously
                    await smtpClient.SendMailAsync(mailMessage);

                    _logger.LogInformation($"Email sent successfully to {email}");

               }
               catch (SmtpException smtpEx)
               {
                    _logger.LogError($"SMTP Error: {smtpEx.Message}");
                    _logger.LogError($"StackTrace: {smtpEx.StackTrace}");
               }
               catch (Exception ex)
               {
                    _logger.LogError($"Error sending email: {ex.Message}");
                    _logger.LogError($"StackTrace: {ex.StackTrace}");
               }
          }
     }
}
