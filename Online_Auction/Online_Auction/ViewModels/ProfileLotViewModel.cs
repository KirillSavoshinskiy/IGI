using System.Collections.Generic;
using Online_Auction.Models;

namespace Online_Auction.ViewModels
{
    public class ProfileLotViewModel
    {
        public Lot Lot { get; set; }
        public List<Comment> Comments { get; set; }
    }
}