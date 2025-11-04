using System.ComponentModel.DataAnnotations;

namespace Bookstore.Domain.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        [Range(1, 5)]
        public int Rating { get; set; }

        public required Book Book { get; set; }
    }
}
