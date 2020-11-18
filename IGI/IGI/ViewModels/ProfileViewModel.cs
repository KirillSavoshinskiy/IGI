﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using IGI.Models;

namespace IGI.ViewModels
{
    public class ProfileViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }   
        public bool EmailConfirmed { get; set; } 
        public IList<string> UserRoles { get; set; }
        public List<Lot> Lots { get; set; }
        public ProfileViewModel()
        { 
            UserRoles = new List<string>();
        }
    }
}