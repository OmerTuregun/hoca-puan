using HocaPuan.Core.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace HocaPuan.Services;

public class EmailService : IEmailService
{
    private const string BrandName = "Hocanı Yorumla";

    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public Task SendVerificationEmailAsync(string toEmail, string verificationLink) =>
        SendAsync(
            toEmail,
            $"{BrandName} — E-posta Doğrulama",
            BuildVerificationHtml(verificationLink));

    public Task SendPasswordResetEmailAsync(string toEmail, string resetLink) =>
        SendAsync(
            toEmail,
            $"{BrandName} — Şifre Sıfırlama",
            BuildPasswordResetHtml(resetLink));

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var host = _config["Email:Host"]
            ?? throw new InvalidOperationException("Email:Host yapılandırılmamış.");
        var port = int.Parse(_config["Email:Port"] ?? "587");
        var username = _config["Email:Username"];
        var password = _config["Email:Password"];
        var fromAddress = _config["Email:From"] ?? "noreply@hocapuan.com";
        var parsedFrom = MailboxAddress.Parse(fromAddress);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(BrandName, parsedFrom.Address));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            if (!string.IsNullOrWhiteSpace(username))
                await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            _logger.LogInformation("E-posta gönderildi: {To} — {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "E-posta gönderilemedi: {To} — {Subject}", toEmail, subject);
            throw;
        }
    }

    private static string BuildVerificationHtml(string link) => $@"
<!DOCTYPE html>
<html lang=""tr"">
<head><meta charset=""utf-8""/></head>
<body style=""font-family: Arial, sans-serif; background: #f5f5f5; padding: 24px;"">
  <div style=""max-width: 480px; margin: 0 auto; background: #fff; border-radius: 12px; padding: 32px;"">
    <h1 style=""color: #1e3a5f; font-size: 22px; margin: 0 0 16px;"">{BrandName}</h1>
    <p style=""color: #444; line-height: 1.6;"">Hesabınızı etkinleştirmek için aşağıdaki butona tıklayın:</p>
    <p style=""text-align: center; margin: 28px 0;"">
      <a href=""{link}"" style=""display: inline-block; background: #2563eb; color: #fff; text-decoration: none; padding: 14px 28px; border-radius: 8px; font-weight: bold;"">E-postamı doğrula</a>
    </p>
    <p style=""color: #888; font-size: 12px;"">Bu link 24 saat geçerlidir. Buton çalışmazsa bağlantıyı tarayıcıya yapıştırın:<br/><a href=""{link}"">{link}</a></p>
  </div>
</body>
</html>";

    private static string BuildPasswordResetHtml(string link) => $@"
<!DOCTYPE html>
<html lang=""tr"">
<head><meta charset=""utf-8""/></head>
<body style=""font-family: Arial, sans-serif; background: #f5f5f5; padding: 24px;"">
  <div style=""max-width: 480px; margin: 0 auto; background: #fff; border-radius: 12px; padding: 32px;"">
    <h1 style=""color: #1e3a5f; font-size: 22px; margin: 0 0 16px;"">{BrandName}</h1>
    <p style=""color: #444; line-height: 1.6;"">Şifrenizi sıfırlamak için aşağıdaki butona tıklayın:</p>
    <p style=""text-align: center; margin: 28px 0;"">
      <a href=""{link}"" style=""display: inline-block; background: #2563eb; color: #fff; text-decoration: none; padding: 14px 28px; border-radius: 8px; font-weight: bold;"">Şifremi sıfırla</a>
    </p>
    <p style=""color: #888; font-size: 12px;"">Bu link 1 saat geçerlidir. Buton çalışmazsa bağlantıyı tarayıcıya yapıştırın:<br/><a href=""{link}"">{link}</a></p>
  </div>
</body>
</html>";

}
