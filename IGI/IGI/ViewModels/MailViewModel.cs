﻿using System.ComponentModel.DataAnnotations;

namespace IGI.ViewModels
{
    public class MailViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        [Required]
        [Display(Name = "Тема сообщения")]
        public string Subject { get; set; }
        [Required]
        [Display(Name = "Текст сообщения")]
        public string Message { get; set; }
    }
}