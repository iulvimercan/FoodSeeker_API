using System.Net;
using System.Net.Mail;

namespace FoodSeekerAPI.Services;

public class EmailService(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var host = _configuration["SmtpSettings:Host"] ??
                   throw new InvalidOperationException("SMTP Host is not configured.");
        var port = Convert.ToInt32(_configuration["SmtpSettings:Port"] ??
                                   throw new InvalidOperationException("SMTP Port is not configured."));
        var username = _configuration["SmtpSettings:Username"] ??
                       throw new InvalidOperationException("SMTP Username is not configured.");
        var password = _configuration["SmtpSettings:Password"] ??
                       throw new InvalidOperationException("SMTP Password is not configured.");

        var smtpClient = new SmtpClient(host)
        {
            Port = port,
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(username),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(toEmail);

        try
        {
            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (SmtpException ex)
        {
            Console.WriteLine($"(ERROR) SMTP error occurred: {ex.Message}");
            throw new InvalidOperationException("Failed to send email. Please check SMTP settings and try again.", ex);
        }
    }
}