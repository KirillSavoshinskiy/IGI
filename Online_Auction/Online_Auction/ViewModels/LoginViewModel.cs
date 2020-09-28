using System.ComponentModel.DataAnnotations;

namespace Online_Auction.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Имя юзера")]
        public string UserName { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }
         
        [Display(Name = "Запомнить?")]
        public bool RememberMe { get; set; }
    }
}