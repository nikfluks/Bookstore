using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Bookstore.Domain.Entities;
using Bookstore.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookstore.Application.Services
{
    internal class AuthorService(AppDbContext db, ILogger<AuthorService> logger) : IAuthorService
    {
        public async Task<IEnumerable<AuthorResponse>> GetAllAsync()
        {
            logger.LogInformation("Retrieving all authors");

            try
            {
                var authors = await db.Authors
                    .Select(a => new AuthorResponse(a.Id, a.Name, a.BirthYear))
                    .ToListAsync();

                logger.LogInformation("Retrieved {AuthorCount} authors", authors.Count);
                return authors;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all authors");
                throw;
            }
        }

        public async Task<AuthorResponse?> GetByIdAsync(int id)
        {
            logger.LogInformation("Retrieving author with Id: {AuthorId}", id);

            try
            {
                var author = await db.Authors.FindAsync(id);

                if (author is null)
                {
                    logger.LogWarning("Author with Id: {AuthorId} not found", id);
                    return null;
                }

                logger.LogInformation("Successfully retrieved author with Id: {AuthorId}", id);
                return new AuthorResponse(author.Id, author.Name, author.BirthYear);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving author with Id: {AuthorId}", id);
                throw;
            }
        }

        public async Task<AuthorResponse> CreateAsync(AuthorCreateRequest authorCreate)
        {
            logger.LogInformation("Creating new author: {AuthorName}", authorCreate.Name);

            try
            {
                var author = new Author
                {
                    Name = authorCreate.Name,
                    BirthYear = authorCreate.BirthYear
                };
                db.Authors.Add(author);
                await db.SaveChangesAsync();

                logger.LogInformation("Successfully created author with Id: {AuthorId}", author.Id);
                return new AuthorResponse(author.Id, author.Name, author.BirthYear);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating author: {AuthorName}", authorCreate.Name);
                throw;
            }
        }

        public async Task<AuthorResponse?> UpdateAsync(int id, AuthorUpdateRequest authorUpdate)
        {
            logger.LogInformation("Updating author with Id: {AuthorId}", id);

            try
            {
                var author = await db.Authors.FindAsync(id);
                if (author is null)
                {
                    logger.LogWarning("Cannot update - Author with Id: {AuthorId} not found", id);
                    return null;
                }

                var oldName = author.Name;
                var oldBirthYear = author.BirthYear;

                author.Name = authorUpdate.Name;
                author.BirthYear = authorUpdate.BirthYear;
                await db.SaveChangesAsync();

                logger.LogInformation("Updated author. AuthorId: {AuthorId}, OldName: {OldName}, NewName: {NewName}, " +
                    "OldBirthYear: {OldBirthYear}, NewBirthYear: {NewBirthYear}",
                   id, oldName, authorUpdate.Name, oldBirthYear, authorUpdate.BirthYear);

                return new AuthorResponse(author.Id, author.Name, author.BirthYear);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating author with Id: {AuthorId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            logger.LogInformation("Attempting to delete author with Id: {AuthorId}", id);

            try
            {
                var author = await db.Authors.FindAsync(id);
                if (author is null)
                {
                    logger.LogWarning("Cannot delete - Author with Id: {AuthorId} not found", id);
                    return false;
                }

                db.Authors.Remove(author);
                var deleted = await db.SaveChangesAsync() > 0;

                if (deleted)
                {
                    logger.LogInformation("Successfully deleted author with Id: {AuthorId}", id);
                }

                return deleted;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting author with Id: {AuthorId}", id);
                throw;
            }
        }
    }
}
