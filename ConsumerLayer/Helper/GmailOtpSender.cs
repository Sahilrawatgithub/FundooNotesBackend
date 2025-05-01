using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ConsumerLayer.Helper
{
    public class GmailOtpSender
    {
        private readonly IConfiguration _configuration;
        public GmailOtpSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void SendEmail(string to, string subject, string body)
        {
            try
            {
                var smtpUsername = _configuration["EmailSettings:Username"];
                var smtpPassword = _configuration["EmailSettings:Password"];
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:Port"]);

                var smtpClient = new SmtpClient(smtpServer)
                {
                    Port = smtpPort,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true,
                    Timeout = 60000
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("sahilrawat12741@gmail.com"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(to);

                smtpClient.Send(mailMessage);
                Console.WriteLine($"Email sent successfully to: {to}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }
}
