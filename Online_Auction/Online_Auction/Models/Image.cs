namespace Online_Auction.Models
{
    public class Image
    {
        public int Id { get; set; }
        public string ImgPath { get; set; }
        
        public int LotId { get; set; }
        public Lot Lot { get; set; }
    }
}