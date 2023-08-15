using AutoMapper.Internal;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using ServiceBilling.API.Application.Contracts.Infrastructure;
using ServiceBilling.API.Application.Models.Mail;

namespace ServiceBilling.API.Infrastructure.Mail
{
    public class EmailService : IEmailService
    {
        public EmailSettings _emailSettings { get; }

        public EmailService(IOptions<EmailSettings> mailSettings)
        {
            _emailSettings = mailSettings.Value;
        }

        public async Task<bool> SendEmail(Email mail)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            Console.WriteLine($"Sending email to {mail.To}");

            return true;
        }
    }
}
