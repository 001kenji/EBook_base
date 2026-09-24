using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace EBook.Controllers
{
    //Install-Package MailKit
    public class EmailService : IEmailSender
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailMessage = new MimeMessage();
            var fromEmail = _configuration["Smtp:Username"];
            var senderName = _configuration["Smtp:SenderName"] ?? "EBook Management";

            emailMessage.From.Add(new MailboxAddress(senderName, fromEmail));
            emailMessage.To.Add(new MailboxAddress(email, email));
            emailMessage.Subject = subject;

            //clean plain text falllback
            string plainTextcontent = Regex.Replace(htmlMessage,"<[^>]*>", string.Empty);
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage,
                TextBody = plainTextcontent
            };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            // connect to Gmail SMTP
            await client.ConnectAsync(
                _configuration["Smtp:Host"],
                int.Parse(_configuration["Smtp:Port"]),
                SecureSocketOptions.StartTls
            );
            //Authenticate using full Gmail address and 16-character App password
            await client.AuthenticateAsync(fromEmail, _configuration["Smtp:Password"]);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);

        }
    }
}
