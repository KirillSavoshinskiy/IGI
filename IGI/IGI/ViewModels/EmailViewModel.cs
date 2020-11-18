﻿using System.ComponentModel.DataAnnotations;

namespace IGI.ViewModels
{
    public class EmailViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}