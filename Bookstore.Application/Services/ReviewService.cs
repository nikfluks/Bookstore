using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Bookstore.Domain.Entities;
using Bookstore.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Services
{
    internal class ReviewService : IReviewService
    {
        private readonly AppDbContext db;

        public ReviewService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<IEnumerable<ReviewResponse>> GetAllAsync()
        {
            return await db.Reviews
                .Include(r => r.Book)
                .Select(r => new ReviewResponse(r.Id, r.Description, r.Rating, r.Book.Title))
                .ToListAsync();
        }

        public async Task<ReviewResponse?> GetByIdAsync(int id)
        {
            var review = await db.Reviews
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.Id == id);

            return review is null
                ? null
                : new ReviewResponse(review.Id, review.Description, review.Rating, review.Book.Title);
        }

        public async Task<ReviewResponse> CreateAsync(ReviewCreateRequest reviewCreate)
        {
            var book = await db.Books.FindAsync(reviewCreate.BookId);
            if (book is null)
            {
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

            return new ReviewResponse(review.Id, review.Description, review.Rating, book.Title);
        }

        public async Task<ReviewResponse?> UpdateAsync(ReviewUpdateRequest reviewUpdate)
        {
            var review = await db.Reviews
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.Id == reviewUpdate.Id);

            if (review is null) return null;

            review.Description = reviewUpdate.Description;
            review.Rating = reviewUpdate.Rating;
            await db.SaveChangesAsync();

            return new ReviewResponse(review.Id, review.Description, review.Rating, review.Book.Title);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var review = await db.Reviews.FindAsync(id);
            if (review is null) return false;

            db.Reviews.Remove(review);
            return await db.SaveChangesAsync() > 0;
        }
    }
}
