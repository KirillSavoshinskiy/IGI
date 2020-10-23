using System.Collections.Generic;
using Online_Auction.Models;

namespace Online_Auction.ViewModels
{
    public class LotViewModel
    {
        public List<Lot> Lots { get; set; }
        public PageViewModel PageViewModel { get; set; }
    }
}