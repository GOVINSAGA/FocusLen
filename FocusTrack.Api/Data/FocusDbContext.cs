using FocusTrack.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FocusTrack.Api.Data;

public class FocusDbContext : DbContext
{
    public FocusDbContext(DbContextOptions<FocusDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<AppLimit> AppLimits => Set<AppLimit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.Property(u => u.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => s.StartTime);
            entity.HasIndex(s => s.EndTime);
            entity.Property(s => s.AppName).IsRequired().HasMaxLength(256);
            entity.Property(s => s.WindowTitle).HasMaxLength(512);
            entity.HasOne(s => s.User)
                  .WithMany()
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppLimit>(entity =>
        {
            entity.HasKey(a => a.Id);
            // Composite index for fast per-user limit lookups
            entity.HasIndex(a => new { a.UserId, a.AppName }).IsUnique();
            entity.Property(a => a.AppName).IsRequired().HasMaxLength(256);
            entity.HasOne(a => a.User)
                  .WithMany()
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
