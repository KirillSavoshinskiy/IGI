using System.ComponentModel.DataAnnotations;

namespace Online_Auction.ViewModels
{
    public class EmailViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}