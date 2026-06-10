namespace Uni_Connect.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public string Faculty { get; set; }

        // Navigation
        public ICollection<Post> Posts { get; set; }
    }
}
