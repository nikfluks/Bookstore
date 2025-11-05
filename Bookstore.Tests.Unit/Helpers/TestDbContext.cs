using Bookstore.Application.Interfaces;
using Bookstore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Tests.Unit.Helpers
{
    public class TestDbContext : DbContext, IAppDbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Authors)
                .WithMany(a => a.Books);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Genres)
                .WithMany(g => g.Books);

            modelBuilder.Entity<Book>()
                .HasMany(b => b.Reviews)
                .WithOne(r => r.Book)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
