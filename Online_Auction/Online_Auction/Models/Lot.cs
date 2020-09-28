using System;
using System.Collections.Generic;

namespace Online_Auction.Models
{
    public class Lot
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime StartSale { get; set; }
        public DateTime FinishSale { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
        
        public List<Comment> Comments { get; set; }
        
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        
        public List<Image> Images { get; set; }
    }
}