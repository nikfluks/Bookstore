using Bookstore.Application.Models;

namespace Bookstore.Application.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewResponse>> GetAllAsync();
        Task<ReviewResponse?> GetByIdAsync(int id);
        Task<ReviewResponse> CreateAsync(ReviewCreateRequest reviewCreate);
        Task<ReviewResponse?> UpdateAsync(int id, ReviewUpdateRequest reviewUpdate);
        Task<bool> DeleteAsync(int id);
    }
}
