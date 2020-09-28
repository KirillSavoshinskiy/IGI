using System.Collections.Generic;

namespace Online_Auction.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public List<Lot> Lots { get; set; }
    }
}