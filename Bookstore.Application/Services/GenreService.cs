using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Bookstore.Domain.Entities;
using Bookstore.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookstore.Application.Services
{
    internal class GenreService(AppDbContext db, ILogger<GenreService> logger) : IGenreService
    {
        public async Task<IEnumerable<GenreResponse>> GetAllAsync()
        {
            logger.LogInformation("Retrieving all genres");

            try
            {
                var genres = await db.Genres
                    .Select(g => new GenreResponse(g.Id, g.Name))
                    .ToListAsync();

                logger.LogInformation("Retrieved {GenreCount} genres", genres.Count);
                return genres;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all genres");
                throw;
            }
        }

        public async Task<GenreResponse?> GetByIdAsync(int id)
        {
            logger.LogInformation("Retrieving genre with Id: {GenreId}", id);

            try
            {
                var genre = await db.Genres.FindAsync(id);

                if (genre is null)
                {
                    logger.LogWarning("Genre with Id: {GenreId} not found", id);
                    return null;
                }

                logger.LogInformation("Successfully retrieved genre with Id: {GenreId}", id);
                return new GenreResponse(genre.Id, genre.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving genre with Id: {GenreId}", id);
                throw;
            }
        }

        public async Task<GenreResponse> CreateAsync(GenreCreateRequest genreCreate)
        {
            logger.LogInformation("Creating new genre: {GenreName}", genreCreate.Name);

            try
            {
                var genre = new Genre
                {
                    Name = genreCreate.Name
                };
                db.Genres.Add(genre);
                await db.SaveChangesAsync();

                logger.LogInformation("Successfully created genre with Id: {GenreId}", genre.Id);
                return new GenreResponse(genre.Id, genre.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating genre: {GenreName}", genreCreate.Name);
                throw;
            }
        }

        public async Task<GenreResponse?> UpdateAsync(int id, GenreUpdateRequest genreUpdate)
        {
            logger.LogInformation("Updating genre with Id: {GenreId}", id);

            try
            {
                var genre = await db.Genres.FindAsync(id);
                if (genre is null)
                {
                    logger.LogWarning("Cannot update - Genre with Id: {GenreId} not found", id);
                    return null;
                }

                var oldName = genre.Name;
                genre.Name = genreUpdate.Name;
                await db.SaveChangesAsync();

                logger.LogInformation("Updated genre. GenreId: {GenreId}, OldName: {OldName}, NewName: {NewName}",
                    id, oldName, genreUpdate.Name);

                return new GenreResponse(genre.Id, genre.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating genre with Id: {GenreId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            logger.LogInformation("Attempting to delete genre with Id: {GenreId}", id);

            try
            {
                var genre = await db.Genres.FindAsync(id);
                if (genre is null)
                {
                    logger.LogWarning("Cannot delete - Genre with Id: {GenreId} not found", id);
                    return false;
                }

                db.Genres.Remove(genre);
                var deleted = await db.SaveChangesAsync() > 0;

                if (deleted)
                {
                    logger.LogInformation("Successfully deleted genre with Id: {GenreId}", id);
                }

                return deleted;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting genre with Id: {GenreId}", id);
                throw;
            }
        }
    }
}
