using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Achievement> Achievements { get; set; } = null!;
    public DbSet<UserAchievement> UserAchievements { get; set; } = null!;
    public DbSet<UserMapProgress> UserMapProgress { get; set; } = null!;
    public DbSet<MapPoint> MapPoints { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<MessageLike> MessageLikes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- USER ----
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.Username).IsRequired().HasMaxLength(100);
            b.Property(u => u.Email).IsRequired().HasMaxLength(200);

            b.HasIndex(u => u.Email).IsUnique();
            b.HasIndex(u => u.Username).IsUnique();

            b.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- ACHIEVEMENTS ----
        modelBuilder.Entity<Achievement>(b =>
        {
            b.HasKey(a => a.Id);
            b.Property(a => a.Code).IsRequired().HasMaxLength(100);
            b.Property(a => a.Title).IsRequired().HasMaxLength(150);
        });

        // ---- USER-ACHIEVEMENTS (M:M) ----
        modelBuilder.Entity<UserAchievement>(b =>
        {
            b.HasKey(ua => new { ua.UserId, ua.AchievementId });

            b.HasOne(ua => ua.User)
                .WithMany(u => u.UserAchievements)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(ua => ua.Achievement)
                .WithMany(a => a.UserAchievements)
                .HasForeignKey(ua => ua.AchievementId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- USER MAP PROGRESS ----
        modelBuilder.Entity<UserMapProgress>(b =>
        {
            b.HasKey(ump => new { ump.UserId, ump.MapPointId });

            b.HasOne(ump => ump.User)
                .WithMany(u => u.MapProgress)
                .HasForeignKey(ump => ump.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(ump => ump.MapPoint)
                .WithMany(mp => mp.UserProgress) // <-- здесь всё правильно
                .HasForeignKey(ump => ump.MapPointId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        // ---- MESSAGES ----
        modelBuilder.Entity<Message>(b =>
        {
            b.HasKey(m => m.Id);

            b.HasOne(m => m.User)
                .WithMany(u => u.Messages) // <-- Указываем обратную связь
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(m => m.Point)
                .WithMany(p => p.Messages) // <-- ИЗМЕНЕНО (вместо .WithMany())
                .HasForeignKey(m => m.MapPointId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(m => m.Likes)
                .WithOne(l => l.Message)
                .HasForeignKey(l => l.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- MESSAGE LIKES ----
        modelBuilder.Entity<MessageLike>(b =>
        {
            b.HasOne(l => l.User)
                .WithMany() // У User нет обратной коллекции лайков, поэтому WithMany() пустой
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Cascade); // При удалении пользователя - удалить все его лайки
        });

        // ---- REFRESH TOKENS ----
        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.HasKey(rt => rt.Id);
            b.Property(rt => rt.Token).IsRequired().HasMaxLength(512);
            b.Property(rt => rt.DeviceId).IsRequired().HasMaxLength(200);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is User && 
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            ((User)entityEntry.Entity).UpdatedAt = DateTime.UtcNow;

            if (entityEntry.State == EntityState.Added)
            {
                ((User)entityEntry.Entity).CreatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
