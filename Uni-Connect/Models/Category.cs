using System.ComponentModel.DataAnnotations;

namespace Uni_Connect.Models
{
    public class Category
    {
        public int CategoryID { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, StringLength(100)]
        public string Faculty { get; set; }

        // Navigation
        public ICollection<Post> Posts { get; set; }
    }
}
