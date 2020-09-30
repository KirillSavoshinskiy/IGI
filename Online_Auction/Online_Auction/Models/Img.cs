namespace Online_Auction.Models
{
    public class Img
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImgPath { get; set; }
        
        public int LotId { get; set; }
        public Lot Lot { get; set; }
    }
}