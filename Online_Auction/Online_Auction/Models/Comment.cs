namespace Online_Auction.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string CommentText { get; set; }
        
        public string UserId { get; set; }
        public User User { get; set; }
        
        public int LotId { get; set; }
        public Lot Lot { get; set; }
    }
}