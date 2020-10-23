using System.ComponentModel.DataAnnotations;

namespace Online_Auction.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress] 
        [Display(Name = "Email")]
        public string Email { get; set; }
 
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }
 
        [Required]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        [Display(Name = "Повторите пароль")]
        public string ConfirmPassword { get; set; }
 
        public string Code { get; set; }
    }
}