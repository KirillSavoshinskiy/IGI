﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IGI.Models
{
    public class Lot
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Название лота")]
        public string Name { get; set; }
        
        [Required]
        [Display(Name = "Описание лота")]
        public string Description { get; set; }
        
        [Required]
        [Display(Name = "Стартовая цена")]
        public decimal Price { get; set; }
        
        public string UserPriceId { get; set; }
        
        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString="{0:dd-MM-yyyy HH}", ApplyFormatInEditMode=true)]
        [Display(Name = "Дата старта торгов (в формате dd/MM/yyyy HH)")]
        public DateTime StartSale { get; set; }
        
        [Required]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString="{0:dd-MM-yyyy HH}", ApplyFormatInEditMode=true)]
        [Display(Name = "Дата завершения торгов (в формате dd/MM/yyyy HH)")]
        public DateTime FinishSale { get; set; }
        
        public bool SentEmail { get; set; }
        
        public string UserId { get; set; }
        public User User { get; set; } 
        
        public int? CategoryId { get; set; }
        public Category Category { get; set; }
        
        public List<Img> Images { get; set; }
        
        public int Hours { get; set; }
    }
}