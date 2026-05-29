namespace Uni_Connect.Models
{
    public class PostTag
    {

        public int PostID { get; set; }
        public int TagID { get; set; }

        // Navigation
        public Post Post { get; set; }
        public Tag Tag { get; set; }
    }
}
