using System.Collections.Generic;

namespace Uni_Connect.Models
{
    public class DashboardViewModel
    {
        public User CurrentUser { get; set; }
        public List<Post> RecentPosts { get; set; }
        public List<User> TopContributors { get; set; }
        public int UnreadNotificationsCount { get; set; }
    }
}
