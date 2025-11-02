using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Bookstore.Domain.Entities;
using Bookstore.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Services
{
    internal class AuthorService : IAuthorService
    {
        private readonly AppDbContext db;

        public AuthorService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<IEnumerable<AuthorResponse>> GetAllAsync()
        {
            return await db.Authors
                .Select(a => new AuthorResponse(a.Id, a.Name, a.BirthYear))
                .ToListAsync();
        }

        public async Task<AuthorResponse?> GetByIdAsync(int id)
        {
            var author = await db.Authors.FindAsync(id);
            return author is null
                ? null
                : new AuthorResponse(author.Id, author.Name, author.BirthYear);
        }

        public async Task<AuthorResponse> CreateAsync(AuthorCreateRequest authorCreate)
        {
            var author = new Author
            {
                Name = authorCreate.Name,
                BirthYear = authorCreate.BirthYear
            };
            db.Authors.Add(author);
            await db.SaveChangesAsync();

            return new AuthorResponse(author.Id, author.Name, author.BirthYear);
        }

        public async Task<AuthorResponse?> UpdateAsync(AuthorUpdateRequest authorUpdate)
        {
            var author = await db.Authors.FindAsync(authorUpdate.Id);
            if (author is null) return null;

            author.Name = authorUpdate.Name;
            author.BirthYear = authorUpdate.BirthYear;
            await db.SaveChangesAsync();

            return new AuthorResponse(author.Id, author.Name, author.BirthYear);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var author = await db.Authors.FindAsync(id);
            if (author is null) return false;

            db.Authors.Remove(author);
            return await db.SaveChangesAsync() > 0;
        }
    }
}
