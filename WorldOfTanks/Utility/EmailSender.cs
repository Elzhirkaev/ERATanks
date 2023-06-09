﻿using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace WorldOfTanks.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(email, subject, htmlMessage);
        }
        public async Task Execute(string email, string subject, string body)
        {
            using var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("ERATanks", "lollolenkovich@mail.ru"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = body
            };
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.mail.ru", 465);
                await client.AuthenticateAsync("lollolenkovich@mail.ru", "**************");
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            };
        }
    }


}
