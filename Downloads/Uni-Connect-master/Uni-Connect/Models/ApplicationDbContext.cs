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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

        }
    }
}
