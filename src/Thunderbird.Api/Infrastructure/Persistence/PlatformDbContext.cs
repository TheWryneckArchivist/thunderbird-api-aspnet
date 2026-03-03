using Microsoft.EntityFrameworkCore;
using Thunderbird.Api.Infrastructure.Outbox;
using Thunderbird.Api.Infrastructure.Persistence.Entities;

namespace Thunderbird.Api.Infrastructure.Persistence;

public sealed class PlatformDbContext : DbContext
{
  public PlatformDbContext(DbContextOptions<PlatformDbContext> options) : base(options)
  {
  }

  public DbSet<UserAccount> UserAccounts => Set<UserAccount>();
  public DbSet<AuthIdentity> AuthIdentities => Set<AuthIdentity>();
  public DbSet<AuthSession> AuthSessions => Set<AuthSession>();
  public DbSet<DiscordOAuthState> DiscordOAuthStates => Set<DiscordOAuthState>();
  public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<UserAccount>(entity =>
    {
      entity.HasKey(x => x.Id);
      entity.Property(x => x.DisplayName).HasMaxLength(64).IsRequired();
      entity.Property(x => x.CreatedAtUnix).IsRequired();
      entity.Property(x => x.UpdatedAtUnix).IsRequired();
      entity.HasMany(x => x.Identities)
        .WithOne(x => x.User)
        .HasForeignKey(x => x.UserId)
        .OnDelete(DeleteBehavior.Cascade);
      entity.HasMany(x => x.Sessions)
        .WithOne(x => x.User)
        .HasForeignKey(x => x.UserId)
        .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<AuthIdentity>(entity =>
    {
      entity.HasKey(x => x.Id);
      entity.Property(x => x.Provider).HasMaxLength(32).IsRequired();
      entity.Property(x => x.ProviderUserId).HasMaxLength(128).IsRequired();
      entity.Property(x => x.DisplayName).HasMaxLength(64).IsRequired();
      entity.Property(x => x.LinkedAtUnix).IsRequired();
      entity.Property(x => x.UpdatedAtUnix).IsRequired();
      entity.HasIndex(x => new { x.Provider, x.ProviderUserId }).IsUnique();
      entity.HasIndex(x => x.UserId);
    });

    modelBuilder.Entity<AuthSession>(entity =>
    {
      entity.HasKey(x => x.Id);
      entity.Property(x => x.RefreshTokenHash).HasMaxLength(128).IsRequired();
      entity.Property(x => x.DeviceFingerprint).HasMaxLength(128).IsRequired();
      entity.Property(x => x.CreatedAtUnix).IsRequired();
      entity.Property(x => x.ExpiresAtUnix).IsRequired();
      entity.Property(x => x.LastIdempotencyKey).HasMaxLength(128);
      entity.HasIndex(x => x.RefreshTokenHash).IsUnique();
      entity.HasIndex(x => new { x.UserId, x.ExpiresAtUnix });
    });

    modelBuilder.Entity<DiscordOAuthState>(entity =>
    {
      entity.HasKey(x => x.Id);
      entity.Property(x => x.State).HasMaxLength(64).IsRequired();
      entity.Property(x => x.RedirectUri).HasMaxLength(1024).IsRequired();
      entity.Property(x => x.Platform).IsRequired();
      entity.Property(x => x.CreatedAtUnix).IsRequired();
      entity.Property(x => x.ExpiresAtUnix).IsRequired();
      entity.HasIndex(x => x.State).IsUnique();
      entity.HasIndex(x => x.ExpiresAtUnix);
    });

    modelBuilder.Entity<OutboxEvent>(entity =>
    {
      entity.HasKey(x => x.Id);
      entity.Property(x => x.EventType).HasMaxLength(128).IsRequired();
      entity.Property(x => x.PayloadJson).IsRequired();
      entity.Property(x => x.CreatedAtUnix).IsRequired();
      entity.Property(x => x.IsPublished).IsRequired();
      entity.HasIndex(x => new { x.IsPublished, x.CreatedAtUnix });
    });
  }
}
