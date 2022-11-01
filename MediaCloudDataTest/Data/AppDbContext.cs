using MediaCloud.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
            Database.EnsureCreated();
        }

        public AppDbContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Preview>().Property(x => x.UpdatedAt).HasDefaultValueSql("getdate()");
            modelBuilder.Entity<Preview>().Property(x => x.CreatedAt).HasDefaultValueSql("getdate()");

            modelBuilder.Entity<Media>().Property(x => x.UpdatedAt).HasDefaultValueSql("getdate()");
            modelBuilder.Entity<Media>().Property(x => x.CreatedAt).HasDefaultValueSql("getdate()");

            modelBuilder.Entity<Tag>().Property(x => x.UpdatedAt).HasDefaultValueSql("getdate()");
            modelBuilder.Entity<Tag>().Property(x => x.CreatedAt).HasDefaultValueSql("getdate()");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is Entity && (
                        e.State == EntityState.Added
                        || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                ((Entity)entityEntry.Entity).UpdatedAt = DateTime.Now;

                if (entityEntry.State == EntityState.Added)
                {
                    ((Entity)entityEntry.Entity).CreatedAt = DateTime.Now;
                }
            }

            return base.SaveChanges();
        }

        public DbSet<Actor> Actors { get; set; }
        public DbSet<Preview> Previews { get; set; }
        public DbSet<Media> Medias { get; set; }
        public DbSet<Tag> Tags { get; set; }
    }
}
