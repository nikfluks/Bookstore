namespace Bookstore.Application.Models;

public class ReviewCreateResult
{
    public bool IsSuccessful { get; set; }
    public ReviewResponse? Review { get; set; }
    public string? ErrorMessage { get; set; }

    public static ReviewCreateResult Success(ReviewResponse review)
    {
        return new ReviewCreateResult { IsSuccessful = true, Review = review };
    }

    public static ReviewCreateResult Failure(string errorMessage)
    {
        return new ReviewCreateResult
        {
            IsSuccessful = false,
            ErrorMessage = errorMessage
        };
    }
}
