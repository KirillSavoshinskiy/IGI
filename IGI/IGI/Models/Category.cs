﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IGI.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Название категории")]
        public string Name { get; set; }
        
        public List<Lot> Lots { get; set; }
    }
}