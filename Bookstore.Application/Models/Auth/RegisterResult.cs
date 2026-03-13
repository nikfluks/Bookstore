namespace Bookstore.Application.Models.Auth;

public class RegisterResult
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }

    public static RegisterResult Success()
    {
        return new RegisterResult { IsSuccessful = true };
    }

    public static RegisterResult Failure(string errorMessage)
    {
        return new RegisterResult
        {
            IsSuccessful = false,
            ErrorMessage = errorMessage
        };
    }
}
