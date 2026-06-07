using Microsoft.EntityFrameworkCore;
using System;

namespace Uni_Connect.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<PostTag> PostTags { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<PrivateSession> PrivateSessions { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PointsTransaction> PointsTransactions { get; set; }
        public DbSet<PostVote> PostVotes { get; set; }
        public DbSet<AnswerVote> AnswerVotes { get; set; }
        public DbSet<PostView> PostViews { get; set; }
        public DbSet<BestAnswerRequest> BestAnswerRequests { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Unique Constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UniversityID)
                .IsUnique();
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // Composite Key for PostTag
            modelBuilder.Entity<PostTag>()
                .HasKey(pt => new { pt.PostID, pt.TagID });

            modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Post>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Answer>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Request>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<PrivateSession>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Message>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Notification>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Report>().HasQueryFilter(e => !e.IsDeleted);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.User)
                .WithMany(u => u.Answers)
                .HasForeignKey(a => a.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.Post)
                .WithMany(p => p.PostTags)
                .HasForeignKey(pt => pt.PostID)
                .IsRequired(false); 

            modelBuilder.Entity<PrivateSession>()
                .HasOne(ps => ps.Student)
                .WithMany(u => u.StudentSessions)
                .HasForeignKey(ps => ps.StudentID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PrivateSession>()
                .HasOne(ps => ps.Helper)
                .WithMany(u => u.HelperSessions)
                .HasForeignKey(ps => ps.HelperID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.Owner)
                .WithMany(u => u.Requests)
                .HasForeignKey(r => r.OwnerID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.Recipient)
                .WithMany(u => u.ReceivedRequests)
                .HasForeignKey(r => r.RecipientID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Report>()
                .HasOne(r => r.Reporter)
                .WithMany(u => u.Reports)
                .HasForeignKey(r => r.ReporterID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Actor)
                .WithMany()
                .HasForeignKey(n => n.ActorUserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PointsTransaction>()
                .HasOne(pt => pt.User)
                .WithMany()
                .HasForeignKey(pt => pt.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BestAnswerRequest>()
                 .HasOne(r => r.Post)
                 .WithMany()
                 .HasForeignKey(r => r.PostID)
                 .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BestAnswerRequest>()
                .HasOne(r => r.Answer)
                .WithMany()
                .HasForeignKey(r => r.AnswerID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BestAnswerRequest>()
                .HasOne(r => r.RequestedByUser)
                .WithMany()
                .HasForeignKey(r => r.RequestedByUserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BestAnswerRequest>()
                .HasOne(r => r.ReviewedByAdmin)
                .WithMany()
                .HasForeignKey(r => r.ReviewedByAdminID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PointsTransaction>().HasQueryFilter(e => !e.IsDeleted);

            modelBuilder.Entity<PostVote>().HasKey(pv => new { pv.UserID, pv.PostID });
            modelBuilder.Entity<PostVote>().HasOne(pv => pv.User).WithMany().HasForeignKey(pv => pv.UserID).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<PostVote>().HasOne(pv => pv.Post).WithMany().HasForeignKey(pv => pv.PostID).OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<AnswerVote>().HasKey(av => new { av.UserID, av.AnswerID });
            modelBuilder.Entity<AnswerVote>().HasOne(av => av.User).WithMany().HasForeignKey(av => av.UserID).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<AnswerVote>().HasOne(av => av.Answer).WithMany().HasForeignKey(av => av.AnswerID).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PostView>().HasKey(pv => new { pv.UserID, pv.PostID });
            modelBuilder.Entity<PostView>().HasOne(pv => pv.User).WithMany().HasForeignKey(pv => pv.UserID).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<PostView>().HasOne(pv => pv.Post).WithMany().HasForeignKey(pv => pv.PostID).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BestAnswerRequest>().HasQueryFilter(e => !e.IsDeleted);

        }
    }
}
