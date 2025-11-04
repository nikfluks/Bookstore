using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Bookstore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bookstore.Application.Services
{
    internal class ReviewService(IAppDbContext db, ILogger<ReviewService> logger) : IReviewService
    {
        public async Task<IEnumerable<ReviewResponse>> GetAllAsync()
        {
            logger.LogInformation("Retrieving all reviews");

            try
            {
                var reviews = await db.Reviews
                    .Include(r => r.Book)
                    .Select(r => new ReviewResponse(r.Id, r.Description, r.Rating, r.Book.Title))
                    .ToListAsync();

                logger.LogInformation("Retrieved {ReviewCount} reviews", reviews.Count);
                return reviews;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving all reviews");
                throw;
            }
        }

        public async Task<ReviewResponse?> GetByIdAsync(int id)
        {
            logger.LogInformation("Retrieving review with Id: {ReviewId}", id);

            try
            {
                var review = await db.Reviews
                    .Include(r => r.Book)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (review is null)
                {
                    logger.LogWarning("Review with Id: {ReviewId} not found", id);
                    return null;
                }

                logger.LogInformation("Successfully retrieved review with Id: {ReviewId}", id);
                return new ReviewResponse(review.Id, review.Description, review.Rating, review.Book.Title);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving review with Id: {ReviewId}", id);
                throw;
            }
        }

        public async Task<ReviewResponse> CreateAsync(ReviewCreateRequest reviewCreate)
        {
            logger.LogInformation("Creating new review for book Id: {BookId}", reviewCreate.BookId);

            try
            {
                var book = await db.Books.FindAsync(reviewCreate.BookId);
                if (book is null)
                {
                    logger.LogWarning("Cannot create review - Book with Id: {BookId} not found", reviewCreate.BookId);
                    throw new ArgumentException($"Book with ID {reviewCreate.BookId} not found.");
                }

                var review = new Review
                {
                    Description = reviewCreate.Description,
                    Rating = reviewCreate.Rating,
                    Book = book
                };
                db.Reviews.Add(review);
                await db.SaveChangesAsync();

                logger.LogInformation("Successfully created review with Id: {ReviewId} for book: {BookTitle}", review.Id, book.Title);
                return new ReviewResponse(review.Id, review.Description, review.Rating, book.Title);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating review for book Id: {BookId}", reviewCreate.BookId);
                throw;
            }
        }

        public async Task<ReviewResponse?> UpdateAsync(int id, ReviewUpdateRequest reviewUpdate)
        {
            logger.LogInformation("Updating review with Id: {ReviewId}", id);

            try
            {
                var review = await db.Reviews
                    .Include(r => r.Book)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (review is null)
                {
                    logger.LogWarning("Cannot update - Review with Id: {ReviewId} not found", id);
                    return null;
                }

                var oldRating = review.Rating;
                review.Description = reviewUpdate.Description;
                review.Rating = reviewUpdate.Rating;
                await db.SaveChangesAsync();

                logger.LogInformation("Updated review. ReviewId: {ReviewId}, OldRating: {OldRating}, NewRating: {NewRating}",
                    id, oldRating, reviewUpdate.Rating);

                return new ReviewResponse(review.Id, review.Description, review.Rating, review.Book.Title);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating review with Id: {ReviewId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            logger.LogInformation("Attempting to delete review with Id: {ReviewId}", id);

            try
            {
                var review = await db.Reviews.FindAsync(id);
                if (review is null)
                {
                    logger.LogWarning("Cannot delete - Review with Id: {ReviewId} not found", id);
                    return false;
                }

                db.Reviews.Remove(review);
                var deleted = await db.SaveChangesAsync() > 0;

                if (deleted)
                {
                    logger.LogInformation("Successfully deleted review with Id: {ReviewId}", id);
                }

                return deleted;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting review with Id: {ReviewId}", id);
                throw;
            }
        }
    }
}
