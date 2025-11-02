using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Bookstore.Domain.Entities;
using Bookstore.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Services
{
    internal class GenreService : IGenreService
    {
        private readonly AppDbContext db;

        public GenreService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<IEnumerable<GenreResponse>> GetAllAsync()
        {
            return await db.Genres
                .Select(g => new GenreResponse(g.Id, g.Name))
                .ToListAsync();
        }

        public async Task<GenreResponse?> GetByIdAsync(int id)
        {
            var genre = await db.Genres.FindAsync(id);
            return genre is null
                ? null
                : new GenreResponse(genre.Id, genre.Name);
        }

        public async Task<GenreResponse> CreateAsync(GenreCreateRequest genreCreate)
        {
            var genre = new Genre
            {
                Name = genreCreate.Name
            };
            db.Genres.Add(genre);
            await db.SaveChangesAsync();

            return new GenreResponse(genre.Id, genre.Name);
        }

        public async Task<GenreResponse?> UpdateAsync(GenreUpdateRequest genreUpdate)
        {
            var genre = await db.Genres.FindAsync(genreUpdate.Id);
            if (genre is null) return null;

            genre.Name = genreUpdate.Name;
            await db.SaveChangesAsync();

            return new GenreResponse(genre.Id, genre.Name);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var genre = await db.Genres.FindAsync(id);
            if (genre is null) return false;

            db.Genres.Remove(genre);
            return await db.SaveChangesAsync() > 0;
        }
    }
}
