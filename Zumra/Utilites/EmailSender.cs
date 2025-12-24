using System.Net;
using System.Net.Mail;
using Zumra.IRepositories;
using Zumra.Models;
using Microsoft.Extensions.Options;

namespace Zumra.Utilites;

public class EmailSender : IEmailSender, Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
{
    private readonly EmailSettings _emailSettings;

    public EmailSender(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public Task SendEmailAsync(string email, string subject, string message)
    {
        var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password)
        };

        var mailMessage = new MailMessage(
            from: _emailSettings.From,
            to: email,
            subject: subject,
            body: message
        );
        mailMessage.IsBodyHtml = true;

        return client.SendMailAsync(mailMessage);
    }
}