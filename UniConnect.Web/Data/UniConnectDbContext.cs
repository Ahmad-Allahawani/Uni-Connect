using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniConnect.Web.Models;

namespace UniConnect.Web.Data
{
    public class UniConnectDbContext : IdentityDbContext<User>
    {
        public UniConnectDbContext(DbContextOptions<UniConnectDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostVote> PostVotes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentVote> CommentVotes { get; set; }
        public DbSet<AcademicRequest> AcademicRequests { get; set; }
        public DbSet<Proposal> Proposals { get; set; }
        public DbSet<PrivateSession> PrivateSessions { get; set; }
        public DbSet<SessionRating> SessionRatings { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<PointTransaction> PointTransactions { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Category self-referencing relationship
            builder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Post relationships
            builder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Post>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // PostVote
            builder.Entity<PostVote>()
                .HasOne(pv => pv.Post)
                .WithMany(p => p.Votes)
                .HasForeignKey(pv => pv.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PostVote>()
                .HasOne(pv => pv.User)
                .WithMany()
                .HasForeignKey(pv => pv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment relationships
            builder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // CommentVote
            builder.Entity<CommentVote>()
                .HasOne(cv => cv.Comment)
                .WithMany(c => c.Votes)
                .HasForeignKey(cv => cv.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CommentVote>()
                .HasOne(cv => cv.User)
                .WithMany()
                .HasForeignKey(cv => cv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // AcademicRequest relationships
            builder.Entity<AcademicRequest>()
                .HasOne(ar => ar.User)
                .WithMany(u => u.Requests)
                .HasForeignKey(ar => ar.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AcademicRequest>()
                .HasOne(ar => ar.Category)
                .WithMany(c => c.Requests)
                .HasForeignKey(ar => ar.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Proposal relationships
            builder.Entity<Proposal>()
                .HasOne(p => p.Request)
                .WithMany(ar => ar.Proposals)
                .HasForeignKey(p => p.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Proposal>()
                .HasOne(p => p.User)
                .WithMany(u => u.Proposals)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // PrivateSession relationships
            builder.Entity<PrivateSession>()
                .HasOne(ps => ps.Request)
                .WithMany()
                .HasForeignKey(ps => ps.RequestId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<PrivateSession>()
                .HasOne(ps => ps.Student)
                .WithMany(u => u.SessionsAsStudent)
                .HasForeignKey(ps => ps.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PrivateSession>()
                .HasOne(ps => ps.Tutor)
                .WithMany(u => u.SessionsAsTutor)
                .HasForeignKey(ps => ps.TutorId)
                .OnDelete(DeleteBehavior.Restrict);

            // SessionRating
            builder.Entity<SessionRating>()
                .HasOne(sr => sr.Session)
                .WithMany(ps => ps.Ratings)
                .HasForeignKey(sr => sr.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SessionRating>()
                .HasOne(sr => sr.User)
                .WithMany()
                .HasForeignKey(sr => sr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Message relationships
            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(m => m.Session)
                .WithMany(ps => ps.Messages)
                .HasForeignKey(m => m.SessionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Report relationships
            builder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany(u => u.ReportsMade)
                .HasForeignKey(r => r.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Report>()
                .HasOne(r => r.ReportedUser)
                .WithMany(u => u.ReportsReceived)
                .HasForeignKey(r => r.ReportedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Report>()
                .HasOne(r => r.ReviewedBy)
                .WithMany()
                .HasForeignKey(r => r.ReviewedById)
                .OnDelete(DeleteBehavior.SetNull);

            // PointTransaction
            builder.Entity<PointTransaction>()
                .HasOne(pt => pt.User)
                .WithMany(u => u.PointTransactions)
                .HasForeignKey(pt => pt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserAchievement
            builder.Entity<UserAchievement>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.Achievements)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
