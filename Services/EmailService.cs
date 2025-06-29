using System.Net;
using System.Net.Mail;

namespace FoodSeekerAPI.Services;

// EmailService handles sending emails using SMTP configuration from app settings
public class EmailService(IConfiguration configuration)
{
    // Store configuration injected via constructor
    private readonly IConfiguration _configuration = configuration;

    /// <summary>
    /// Sends an email asynchronously.
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="subject">Email subject line</param>
    /// <param name="body">Email body content (HTML allowed)</param>
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        // Retrieve SMTP settings from configuration, throw if any missing
        var host = _configuration["SmtpSettings:Host"] ??
                   throw new InvalidOperationException("SMTP Host is not configured.");
        var port = Convert.ToInt32(_configuration["SmtpSettings:Port"] ??
                                   throw new InvalidOperationException("SMTP Port is not configured."));
        var username = _configuration["SmtpSettings:Username"] ??
                       throw new InvalidOperationException("SMTP Username is not configured.");
        var password = _configuration["SmtpSettings:Password"] ??
                       throw new InvalidOperationException("SMTP Password is not configured.");

        // Initialize SMTP client with host, port, credentials, and SSL enabled
        var smtpClient = new SmtpClient(host)
        {
            Port = port,
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true,
        };

        // Compose the email message
        var mailMessage = new MailMessage
        {
            From = new MailAddress(username), // Sender email address
            Subject = subject,
            Body = body,
            IsBodyHtml = true, // Support HTML content in email body
        };
        mailMessage.To.Add(toEmail); // Add recipient

        try
        {
            // Send email asynchronously
            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (SmtpException ex)
        {
            // Log SMTP errors and rethrow as InvalidOperationException with message
            Console.WriteLine($"(ERROR) SMTP error occurred: {ex.Message}");
            throw new InvalidOperationException("Failed to send email. Please check SMTP settings and try again.", ex);
        }
    }
}
