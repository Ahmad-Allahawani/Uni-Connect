namespace UniConnect.Web.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public int? SessionId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public bool IsFlagged { get; set; } = false;
        public string? FlagReason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public virtual User? Sender { get; set; }
        public virtual User? Receiver { get; set; }
        public virtual PrivateSession? Session { get; set; }
    }
}
