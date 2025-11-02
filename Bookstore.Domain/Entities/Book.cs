namespace Bookstore.Domain.Entities
{
    public class Book
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public float Price { get; set; }

        public ICollection<Genre> Genres { get; set; } = [];
        public ICollection<Author> Authors { get; set; } = [];
        public ICollection<Review> Reviews { get; set; } = [];
    }
}
