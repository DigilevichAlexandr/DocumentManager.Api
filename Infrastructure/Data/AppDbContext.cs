using DocumentManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DocumentManager.Api.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<DocumentMetadata> DocumentMetadata => Set<DocumentMetadata>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.FilePath).HasMaxLength(1000);
            entity.Property(e => e.FileName).HasMaxLength(500);
            entity.Property(e => e.ContentType).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(2000);
            entity.HasOne(e => e.Document)
                .WithMany(d => d.Comments)
                .HasForeignKey(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DocumentMetadata>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LocationName).HasMaxLength(500);
            entity.HasOne(e => e.Document)
                .WithOne(d => d.Metadata)
                .HasForeignKey<DocumentMetadata>(e => e.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
