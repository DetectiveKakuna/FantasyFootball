using FantasyFootball.Core.Entities;
using FantasyFootball.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FantasyFootball.Infrastructure.Persistence;

public class FantasyFootballDbContext : DbContext
{
    public DbSet<PlayerEntity> Players => Set<PlayerEntity>();
    public DbSet<ExpertEntity> Experts => Set<ExpertEntity>();

    public FantasyFootballDbContext(DbContextOptions<FantasyFootballDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigurePlayerEntity(modelBuilder);
        ConfigureExpertEntity(modelBuilder);
    }

    private static void ConfigurePlayerEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerEntity>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.HasIndex(p => p.SleeperId).IsUnique();
            entity.HasIndex(p => p.FantasyProsId).IsUnique();

            entity.Property(p => p.SleeperId).IsRequired();
            entity.Property(p => p.FirstName).IsRequired();
            entity.Property(p => p.LastName).IsRequired();
            entity.Property(p => p.FullName).IsRequired();

            entity.Property(p => p.Position)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(p => p.Team)
                .HasConversion<string>()
                .IsRequired();

            entity.Property(p => p.InjuryStatus)
                .HasConversion<string>();

            entity.Property(p => p.FantasyPositions)
                .HasConversion(
                    v => string.Join(",", v.Select(p => p.ToString())),
                    v => string.IsNullOrEmpty(v)
                        ? new List<Position>()
                        : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => Enum.Parse<Position>(s))
                            .ToList())
                .Metadata.SetValueComparer(new ValueComparer<List<Position>>(
                    (a, b) => a != null && b != null && a.SequenceEqual(b),
                    c => c.Aggregate(0, (hash, v) => HashCode.Combine(hash, v.GetHashCode())),
                    c => c.ToList()));

            entity.Property(p => p.LastUpdated).IsRequired();
        });
    }

    private static void ConfigureExpertEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExpertEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.Slug).IsUnique();

            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Slug).IsRequired();
            entity.Property(e => e.CustomWeight).HasDefaultValue(1.0);

            entity.OwnsOne(e => e.DraftAccuracy, a =>
            {
                a.Property(x => x.OverallRank).HasColumnName("DraftAccuracy_OverallRank");
                a.Property(x => x.Qb).HasColumnName("DraftAccuracy_Qb");
                a.Property(x => x.Rb).HasColumnName("DraftAccuracy_Rb");
                a.Property(x => x.Wr).HasColumnName("DraftAccuracy_Wr");
                a.Property(x => x.Te).HasColumnName("DraftAccuracy_Te");
                a.Property(x => x.K).HasColumnName("DraftAccuracy_K");
                a.Property(x => x.Dst).HasColumnName("DraftAccuracy_Dst");
                a.Property(x => x.Idp).HasColumnName("DraftAccuracy_Idp");
            });

            entity.OwnsOne(e => e.WeeklyAccuracy, a =>
            {
                a.Property(x => x.OverallRank).HasColumnName("WeeklyAccuracy_OverallRank");
                a.Property(x => x.Qb).HasColumnName("WeeklyAccuracy_Qb");
                a.Property(x => x.Rb).HasColumnName("WeeklyAccuracy_Rb");
                a.Property(x => x.Wr).HasColumnName("WeeklyAccuracy_Wr");
                a.Property(x => x.Te).HasColumnName("WeeklyAccuracy_Te");
                a.Property(x => x.K).HasColumnName("WeeklyAccuracy_K");
                a.Property(x => x.Dst).HasColumnName("WeeklyAccuracy_Dst");
                a.Property(x => x.Idp).HasColumnName("WeeklyAccuracy_Idp");
            });

            var intListComparer = new ValueComparer<List<int>?>(
                (a, b) => a != null && b != null ? a.SequenceEqual(b) : a == b,
                c => c == null ? 0 : c.Aggregate(0, (hash, v) => HashCode.Combine(hash, v)),
                c => c == null ? null : c.ToList());

            entity.Property(e => e.DraftRankings)
                .HasConversion(
                    v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<List<int>>(v, (System.Text.Json.JsonSerializerOptions?)null));
            entity.Property(e => e.DraftRankings).Metadata.SetValueComparer(intListComparer);

            entity.Property(e => e.WeeklyRankings)
                .HasConversion(
                    v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<List<int>>(v, (System.Text.Json.JsonSerializerOptions?)null));
            entity.Property(e => e.WeeklyRankings).Metadata.SetValueComparer(intListComparer);

            entity.Property(e => e.RosRankings)
                .HasConversion(
                    v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<List<int>>(v, (System.Text.Json.JsonSerializerOptions?)null));
            entity.Property(e => e.RosRankings).Metadata.SetValueComparer(intListComparer);
        });
    }
}
