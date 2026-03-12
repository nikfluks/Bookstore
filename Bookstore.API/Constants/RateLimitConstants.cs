namespace Bookstore.API.Constants;

public static class RateLimitConstants
{
    public const string AuthenticatedPolicyName = "authenticated";

    public static readonly TimeSpan GlobalWindow = TimeSpan.FromMinutes(1);
    public const int GlobalPermitLimit = 60;

    public static readonly TimeSpan AuthenticatedWindow = TimeSpan.FromMinutes(1);
    public const int AuthenticatedSegmentsPerWindow = 4;
    public const int AuthenticatedPermitLimit = 100;

    /// <summary>
    /// Duration of one segment. When the limiter rejects a request, the oldest segment expires
    /// within this duration, so it is the tightest valid Retry-After value.
    /// </summary>
    public static TimeSpan AuthenticatedSegmentDuration => AuthenticatedWindow / AuthenticatedSegmentsPerWindow;
}
