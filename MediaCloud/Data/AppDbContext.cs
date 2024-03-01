using MediaCloud.Data.Models;
using MediaCloud.WebApp;
using Microsoft.EntityFrameworkCore;

namespace MediaCloud.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
            if (Database.EnsureCreated())
            {
                var admin = new Actor()
                {
                    Name = "Admin",
                    PasswordHash = SecureHash.Hash("superadmin"),
                    IsActivated = true,
                    IsAdmin = true
                };

                Actors.Add(admin);
                SaveChangesAsync();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }

        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries().Where(e => e.Entity is Record && (
                                                             e.State == EntityState.Added ||
                                                             e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                ((Record)entityEntry.Entity).UpdatedAt = DateTime.Now.ToUniversalTime();

                if (entityEntry.State == EntityState.Added)
                {
                    ((Record)entityEntry.Entity).CreatedAt = DateTime.Now.ToUniversalTime();
                }
            }

            return base.SaveChanges();
        }

        public DbSet<Actor> Actors { get; set; }
        public DbSet<Preview> Previews { get; set; }
        public DbSet<Media> Medias { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<StatisticSnapshot> StatisticSnapshots { get; set; }
    }
}
