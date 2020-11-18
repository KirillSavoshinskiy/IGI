﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace IGI.Models
{
    public class User: IdentityUser
    {
        public List<Lot> Lots { get; set; } 
    }
}