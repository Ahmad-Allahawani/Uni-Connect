namespace Uni_Connect.Models
{
    public class Tag
    {
        public int TagID { get; set; }
        public string Name { get; set; }

        // Navigation
        public ICollection<PostTag> PostTags { get; set; }
    }
}
