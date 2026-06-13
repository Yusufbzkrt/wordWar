using KelimeOyunu.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace KelimeOyunu.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<GameRound> GameRounds => Set<GameRound>();
    public DbSet<Friendship> Friendships => Set<Friendship>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<UserDailyQuest> DailyQuests => Set<UserDailyQuest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
        });

        // Question Configuration
        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
            entity.HasMany(e => e.Answers)
                  .WithOne(a => a.Question)
                  .HasForeignKey(a => a.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Answer Configuration
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).HasMaxLength(200).IsRequired();
        });

        // GameSession Configuration
        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Player1)
                  .WithMany(u => u.GameSessionsAsPlayer1)
                  .HasForeignKey(e => e.Player1Id)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Player2)
                  .WithMany(u => u.GameSessionsAsPlayer2)
                  .HasForeignKey(e => e.Player2Id)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Winner)
                  .WithMany()
                  .HasForeignKey(e => e.WinnerId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // GameRound Configuration
        modelBuilder.Entity<GameRound>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.GameSession)
                  .WithMany(s => s.Rounds)
                  .HasForeignKey(e => e.GameSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Question)
                  .WithMany()
                  .HasForeignKey(e => e.QuestionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Friendship Configuration
        modelBuilder.Entity<Friendship>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Requester)
                  .WithMany(u => u.SentFriendRequests)
                  .HasForeignKey(e => e.RequesterId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Addressee)
                  .WithMany(u => u.ReceivedFriendRequests)
                  .HasForeignKey(e => e.AddresseeId)
                  .OnDelete(DeleteBehavior.Restrict);
            // Aynı iki kişi arasında sadece bir arkadaşlık
            entity.HasIndex(e => new { e.RequesterId, e.AddresseeId }).IsUnique();
        });

        // Message Configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).HasMaxLength(1000).IsRequired();
            entity.HasOne(e => e.Sender)
                  .WithMany(u => u.SentMessages)
                  .HasForeignKey(e => e.SenderId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Receiver)
                  .WithMany(u => u.ReceivedMessages)
                  .HasForeignKey(e => e.ReceiverId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.SentAt);
        });

        // Transaction Configuration
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Transactions)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.CreatedAt);
        });

        // DailyQuest Configuration
        modelBuilder.Entity<UserDailyQuest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany(u => u.DailyQuests)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.AssignedDate });
        });

        // Seed Data
        SeedData.Seed(modelBuilder);
    }
}
