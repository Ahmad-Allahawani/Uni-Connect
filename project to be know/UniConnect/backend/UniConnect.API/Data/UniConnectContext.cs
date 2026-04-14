using Microsoft.EntityFrameworkCore;
using UniConnect.API.Models;

namespace UniConnect.API.Data;

public class UniConnectContext : DbContext
{
    public UniConnectContext(DbContextOptions<UniConnectContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<AcademicRequest> AcademicRequests { get; set; }
    public DbSet<Proposal> Proposals { get; set; }
    public DbSet<PointsTransaction> PointsTransactions { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<LearningSession> LearningSessions { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>()
            .HasKey(u => u.Id);
        modelBuilder.Entity<User>()
            .HasIndex(u => u.UniversityId)
            .IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Post configuration
        modelBuilder.Entity<Post>()
            .HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Comment configuration
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // AcademicRequest configuration
        modelBuilder.Entity<AcademicRequest>()
            .HasOne(ar => ar.User)
            .WithMany(u => u.AcademicRequests)
            .HasForeignKey(ar => ar.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Proposal configuration
        modelBuilder.Entity<Proposal>()
            .HasOne(p => p.AcademicRequest)
            .WithMany(ar => ar.Proposals)
            .HasForeignKey(p => p.AcademicRequestId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Proposal>()
            .HasOne(p => p.User)
            .WithMany(u => u.Proposals)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // PointsTransaction configuration
        modelBuilder.Entity<PointsTransaction>()
            .HasOne(pt => pt.User)
            .WithMany(u => u.PointsTransactions)
            .HasForeignKey(pt => pt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Message configuration
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Message>()
            .HasOne(m => m.LearningSession)
            .WithMany(ls => ls.Messages)
            .HasForeignKey(m => m.LearningSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // LearningSession configuration
        modelBuilder.Entity<LearningSession>()
            .HasOne(ls => ls.Initiator)
            .WithMany(u => u.InitiatedSessions)
            .HasForeignKey(ls => ls.InitiatorId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<LearningSession>()
            .HasOne(ls => ls.Acceptor)
            .WithMany(u => u.AcceptedSessions)
            .HasForeignKey(ls => ls.AcceptorId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<LearningSession>()
            .HasOne(ls => ls.Post)
            .WithMany()
            .HasForeignKey(ls => ls.PostId)
            .OnDelete(DeleteBehavior.SetNull);

        // Report configuration
        modelBuilder.Entity<Report>()
            .HasOne(r => r.ReportedBy)
            .WithMany(u => u.SubmittedReports)
            .HasForeignKey(r => r.ReportedById)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Report>()
            .HasOne(r => r.Post)
            .WithMany(p => p.Reports)
            .HasForeignKey(r => r.PostId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Report>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
