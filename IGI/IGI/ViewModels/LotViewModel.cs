﻿using System.Collections.Generic;
using IGI.Models;

namespace IGI.ViewModels
{
    public class LotViewModel
    {
        public List<Lot> Lots { get; set; }
        public PageViewModel PageViewModel { get; set; }
    }
}