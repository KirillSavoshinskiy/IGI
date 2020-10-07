using System.Threading.Tasks;

namespace Online_Auction.Services
{
    public interface IEmailService
    {
        public Task SendEmailAsync(string email, string subject, string mess);
    }
}