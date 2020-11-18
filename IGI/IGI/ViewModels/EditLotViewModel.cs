﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using IGI.Models;

namespace IGI.ViewModels
{
    public class EditLotViewModel
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

        public int Now { get; set; } 
        public int? CategoryId { get; set; }  
        
        public IFormFileCollection Images { get; set; }
    }
}