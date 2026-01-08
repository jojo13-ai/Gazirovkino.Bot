using Gazirovkino.Bot.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gazirovkino.Bot.Data;

public class GazirovkinoDbContext : DbContext
{
    public GazirovkinoDbContext(DbContextOptions<GazirovkinoDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Survey> Surveys => Set<Survey>();
    public DbSet<Criteria> Criteria => Set<Criteria>();
    public DbSet<Interview> Interviews => Set<Interview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<Survey>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserId).IsRequired();
            entity.Property(x => x.DateCreated).IsRequired();
            entity.Property(x => x.Status).IsRequired();

            entity.HasIndex(x => x.UserId);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Criteria>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SurveyId).IsRequired();
            entity.Property(x => x.DateCreated).IsRequired();
            entity.Property(x => x.Type).IsRequired();
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.Order).IsRequired();

            entity.HasIndex(x => x.SurveyId);
            entity.HasIndex(x => new { x.SurveyId, x.Order }).IsUnique();

            entity.HasOne<Survey>()
                .WithMany()
                .HasForeignKey(x => x.SurveyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Interview>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserId).IsRequired();

            entity.HasIndex(x => x.UserId);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
