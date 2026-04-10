namespace Uni_Connect.ViewModels
{
    public class PostViewModel
    {
        public int PostID { get; set; }
        public string AuthorName { get; set; }
        public string AuthorUsername { get; set; }
        public string CategoryName { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Upvotes { get; set; }
        public int ViewsCount { get; set; }
        public int AnswerCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Tags { get; set; }= new();
    }
}
