﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Online_Auction.Models
{
    public class User: IdentityUser
    {
        public List<Lot> Lots { get; set; }
        public List<Comment> Comments { get; set; }
    }
}