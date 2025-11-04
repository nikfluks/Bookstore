using Bookstore.Application.Models;

namespace Bookstore.Application.Interfaces
{
    public interface IThirdPartyBookApiService
    {
        Task<IEnumerable<ImportBookDto>> FetchBooksAsync();
    }
}
