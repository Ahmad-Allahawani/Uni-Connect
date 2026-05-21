using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Utils;

public class EmailService
{
    private readonly IConfiguration _config;
    public EmailService(IConfiguration config) => _config = config;

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Uni-Connect", _config["Email:From"]));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        message.MessageId = MimeUtils.GenerateMessageId();
        message.Headers.Add("X-Mailer", "Uni-Connect App");

        var builder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = System.Text.RegularExpressions.Regex.Replace(
                htmlBody, "<[^>]+>", " ").Trim()
        };
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_config["Email:Host"], int.Parse(_config["Email:Port"]!),
            SecureSocketOptions.SslOnConnect);
        await client.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}