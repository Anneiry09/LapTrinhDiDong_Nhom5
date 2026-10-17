using System.Net;
using System.Net.Mail;
using AnFit.Api.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AnFit.Api.Services.Implementations.Core
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            var smtpSettings = _config.GetSection("SmtpSettings");
            var host = smtpSettings["Host"];
            var portStr = smtpSettings["Port"];
            var senderName = smtpSettings["SenderName"] ?? "AnFit Assistant";
            var senderEmail = smtpSettings["SenderEmail"] ?? "no-reply@anfit.vn";
            var username = smtpSettings["Username"];
            var password = smtpSettings["Password"];

            // Nếu thiếu cấu hình SMTP, in thẳng ra Console và không ném lỗi
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                LogFallbackEmail(toEmail, subject, body);
                return;
            }

            int port = int.TryParse(portStr, out int p) ? p : 587;

            try
            {
                using var client = new SmtpClient(host, port)
                {
                    Port = port,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(username, password.Replace(" ", "")),
                    EnableSsl = true,
                    Timeout = 8000
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Đã gửi email thành công tới: {Email}", toEmail);
            }
            catch (Exception ex)
            {
                // Khi Gmail chặn kết nối (như app password hết hạn, mạng trường chặn port 587)
                // Ta ghi log mã OTP ra Console để bạn vẫn có OTP test và không làm văng lỗi 500
                _logger.LogWarning("Không thể gửi email qua SMTP: {Message}. Kích hoạt chế độ Development Console Fallback.", ex.Message);
                LogFallbackEmail(toEmail, subject, body);
            }
        }

        private void LogFallbackEmail(string toEmail, string subject, string body)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("================================================================================");
            Console.WriteLine("[EMAIL SERVICE - CONSOLE FALLBACK / TEST ĐỒ ÁN]");
            Console.WriteLine($"Gửi đến: {toEmail}");
            Console.WriteLine($"Tiêu đề: {subject}");
            Console.WriteLine($"Nội dung / OTP: {body}");
            Console.WriteLine("================================================================================");
            Console.ResetColor();
        }
    }
}
