namespace Bookstore.Domain.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        /// <summary>
        /// Allowed values are from 1 to 5
        /// </summary>
        public int Rating { get; set; }

        public required Book Book { get; set; }
    }
}
