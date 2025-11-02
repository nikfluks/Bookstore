using Bookstore.Application.Interfaces;
using Bookstore.Application.Models;
using Bookstore.Domain.Entities;
using Bookstore.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Application.Services
{
    internal class BookService : IBookService
    {
        private readonly AppDbContext db;

        public BookService(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<IEnumerable<BookResponse>> GetAllAsync()
        {
            return await db.Books
                .Select(b => new BookResponse(b.Id, b.Title, b.Price))
                .ToListAsync();
        }

        public async Task<BookResponse?> GetByIdAsync(int id)
        {
            var book = await db.Books.FindAsync(id);
            return book is null
                ? null
                : new BookResponse(book.Id, book.Title, book.Price);
        }

        public async Task<BookResponse> CreateAsync(BookCreateRequest bookCreate)
        {
            var book = new Book
            {
                Title = bookCreate.Title,
                Price = bookCreate.Price
            };
            db.Books.Add(book);
            await db.SaveChangesAsync();

            return new BookResponse(book.Id, book.Title, book.Price);
        }

        public async Task<BookResponse?> UpdateAsync(BookUpdateRequest bookUpdate)
        {
            var book = await db.Books.FindAsync(bookUpdate.Id);
            if (book is null) return null;

            book.Price = bookUpdate.Price;
            await db.SaveChangesAsync();

            return new BookResponse(book.Id, book.Title, book.Price);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var book = await db.Books.FindAsync(id);
            if (book is null) return false;

            db.Books.Remove(book);
            return await db.SaveChangesAsync() > 0;
        }
    }
}
