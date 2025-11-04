namespace Bookstore.Application.Interfaces
{
    public interface IBookImportService
    {
        Task<int> ImportBooksAsync();
    }
}
