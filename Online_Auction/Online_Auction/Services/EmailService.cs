using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;

namespace Online_Auction.Services
{
    public class EmailService: IEmailService
    { 
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
                await client.ConnectAsync("smtp.gmail.com", 587, false);//afrter change ssl true
                await client.AuthenticateAsync("IgiOnlineAuction@gmail.com", "Password123#");
                await client.SendAsync(emailMess);
                await client.DisconnectAsync(true);
            }
        }
    }
}