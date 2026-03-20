namespace Bookstore.API.Middleware;

public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(static state =>
        {
            var headers = ((HttpContext)state).Response.Headers;

            // Prevent browsers from MIME-sniffing JSON responses as HTML
            headers.Append("X-Content-Type-Options", "nosniff");

            // Prevent this API from being embedded in frames (defense-in-depth)
            headers.Append("Content-Security-Policy", "default-src 'none'; frame-ancestors 'none'");

            return Task.CompletedTask;
        }, context);

        await next(context);
    }
}
