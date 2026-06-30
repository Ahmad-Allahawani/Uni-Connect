using Microsoft.EntityFrameworkCore;
using UniConnect.Web.Data;
using UniConnect.Web.Models;

namespace UniConnect.Web.Services
{
    public interface ISearchService
    {
        Task<SearchResult> SearchAsync(string query, string? category = null, SearchType? type = null);
        Task<List<Post>> SearchPostsAsync(string query, string? category = null);
        Task<List<AcademicRequest>> SearchRequestsAsync(string query, string? category = null);
        Task<List<User>> SearchUsersAsync(string query);
    }

    public class SearchService : ISearchService
    {
        private readonly UniConnectDbContext _context;

        public SearchService(UniConnectDbContext context)
        {
            _context = context;
        }

        public async Task<SearchResult> SearchAsync(string query, string? category = null, SearchType? type = null)
        {
            var result = new SearchResult();

            if (string.IsNullOrWhiteSpace(query))
            {
                return result;
            }

            // Search posts if no type specified or if posts requested
            if (type == null || type == SearchType.Posts)
            {
                result.Posts = await SearchPostsAsync(query, category);
            }

            // Search requests if no type specified or if requests requested
            if (type == null || type == SearchType.Requests)
            {
                result.Requests = await SearchRequestsAsync(query, category);
            }

            // Search users if no type specified or if users requested
            if (type == null || type == SearchType.Users)
            {
                result.Users = await SearchUsersAsync(query);
            }

            return result;
        }

        public async Task<List<Post>> SearchPostsAsync(string query, string? category = null)
        {
            var queryLower = query.ToLower();

            var postsQuery = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Comments)
                .Where(p => (p.Title.ToLower().Contains(queryLower) || 
                            p.Content.ToLower().Contains(queryLower)) &&
                            (category == null || p.Category != null && p.Category.Name == category))
                .OrderByDescending(p => p.CreatedAt)
                .Take(50);

            return await postsQuery.ToListAsync();
        }

        public async Task<List<AcademicRequest>> SearchRequestsAsync(string query, string? category = null)
        {
            var queryLower = query.ToLower();

            var requestsQuery = _context.AcademicRequests
                .Include(r => r.User)
                .Include(r => r.Category)
                .Include(r => r.Proposals)
                .Where(r => (r.Title.ToLower().Contains(queryLower) || 
                            r.Description.ToLower().Contains(queryLower)) &&
                            (category == null || r.Category != null && r.Category.Name == category))
                .OrderByDescending(r => r.CreatedAt)
                .Take(50);

            return await requestsQuery.ToListAsync();
        }

        public async Task<List<User>> SearchUsersAsync(string query)
        {
            var queryLower = query.ToLower();

            var usersQuery = _context.Users
                .Where(u => u.FirstName.ToLower().Contains(queryLower) ||
                           u.LastName.ToLower().Contains(queryLower) ||
                           u.UniversityId.ToLower().Contains(queryLower) ||
                           u.Email.ToLower().Contains(queryLower))
                .OrderByDescending(u => u.Points)
                .Take(20);

            return await usersQuery.ToListAsync();
        }
    }

    public class SearchResult
    {
        public List<Post> Posts { get; set; } = new();
        public List<AcademicRequest> Requests { get; set; } = new();
        public List<User> Users { get; set; } = new();
    }

    public enum SearchType
    {
        Posts,
        Requests,
        Users
    }
}
