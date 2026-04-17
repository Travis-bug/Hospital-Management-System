using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Hospital_Management_System.Services.Infrastructure;

public sealed class SmtpEmailSender(
    IConfiguration configuration,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var host = configuration["Email:Smtp:Host"];
        var portValue = configuration["Email:Smtp:Port"];
        var username = configuration["Email:Smtp:Username"];
        var password = configuration["Email:Smtp:Password"];
        var fromAddress = configuration["Email:Smtp:FromAddress"];
        var fromName = configuration["Email:Smtp:FromName"] ?? "Hospital Management System";
        var enableSsl = bool.TryParse(configuration["Email:Smtp:EnableSsl"], out var sslEnabled) && sslEnabled;

        if (string.IsNullOrWhiteSpace(host)
            || string.IsNullOrWhiteSpace(portValue)
            || string.IsNullOrWhiteSpace(fromAddress))
        {
            throw new InvalidOperationException(
                "SMTP email settings are incomplete. Configure Email:Smtp:Host, Port, and FromAddress.");
        }

        if (!int.TryParse(portValue, out var port))
        {
            throw new InvalidOperationException("Email:Smtp:Port must be a valid integer.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(fromAddress, fromName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };

        message.To.Add(email);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl
        };

        if (!string.IsNullOrWhiteSpace(username))
        {
            client.Credentials = new NetworkCredential(username, password);
        }
        else
        {
            client.UseDefaultCredentials = true;
        }

        logger.LogInformation("Sending email to {Email} via configured SMTP host {Host}.", email, host);
        await client.SendMailAsync(message);
    }
}
