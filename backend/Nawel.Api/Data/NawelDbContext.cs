using Microsoft.EntityFrameworkCore;
using Nawel.Api.Models;

namespace Nawel.Api.Data;

public class NawelDbContext : DbContext
{
    public NawelDbContext(DbContextOptions<NawelDbContext> options) : base(options)
    {
    }

    public DbSet<Family> Families { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<GiftList> Lists { get; set; }
    public DbSet<Gift> Gifts { get; set; }
    public DbSet<GiftParticipation> GiftParticipations { get; set; }
    public DbSet<OpenGraphRequest> OpenGraphRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Family configuration
        modelBuilder.Entity<Family>(entity =>
        {
            entity.HasIndex(e => e.Name);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Login).IsUnique();
            entity.HasIndex(e => e.Email);

            entity.HasOne(u => u.Family)
                .WithMany(f => f.Users)
                .HasForeignKey(u => u.FamilyId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(u => u.List)
                .WithOne(l => l.User)
                .HasForeignKey<GiftList>(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // GiftList configuration
        modelBuilder.Entity<GiftList>(entity =>
        {
            entity.HasIndex(e => e.UserId).IsUnique();
        });

        // Gift configuration
        modelBuilder.Entity<Gift>(entity =>
        {
            entity.HasIndex(e => e.ListId);
            entity.HasIndex(e => e.Year);
            entity.HasIndex(e => e.TakenBy);

            entity.HasOne(g => g.List)
                .WithMany(l => l.Gifts)
                .HasForeignKey(g => g.ListId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(g => g.TakenByUser)
                .WithMany(u => u.TakenGifts)
                .HasForeignKey(g => g.TakenBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // GiftParticipation configuration
        modelBuilder.Entity<GiftParticipation>(entity =>
        {
            entity.HasIndex(e => new { e.GiftId, e.UserId });

            entity.HasOne(gp => gp.Gift)
                .WithMany(g => g.Participations)
                .HasForeignKey(gp => gp.GiftId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gp => gp.User)
                .WithMany(u => u.GiftParticipations)
                .HasForeignKey(gp => gp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // OpenGraphRequest configuration
        modelBuilder.Entity<OpenGraphRequest>(entity =>
        {
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                var createdAtValue = entry.Property("CreatedAt").CurrentValue;
                if (createdAtValue == null || (createdAtValue is DateTime dt && dt == default))
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                }
            }

            if (entry.Properties.Any(p => p.Metadata.Name == "UpdatedAt"))
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        }
    }
}
