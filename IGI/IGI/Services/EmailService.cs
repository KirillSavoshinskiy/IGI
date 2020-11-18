﻿using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace IGI.Services
{
    public class EmailService: IEmailService
    {
        private IConfiguration Configuration;
        public EmailService(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        public async Task SendEmailAsync(string email, string subject, string mess)
        {
            var emailMess = new MimeMessage();
            emailMess.From.Add(new MailboxAddress("Online_Auction", "IgiOnlineAuction@gmail.com"));
            emailMess.To.Add(new MailboxAddress("", email));
            emailMess.Subject = subject;
            emailMess.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = mess
            };
        
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, false); 
                var ema = Configuration["ServerEmail:email"];
                await client.AuthenticateAsync(Configuration["ServerEmail:email"],
                    Configuration["ServerEmail:password"]);
                await client.SendAsync(emailMess);
                await client.DisconnectAsync(true);
            }
        }
    }
}