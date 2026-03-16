using Bookstore.Application.Models.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Bookstore.Tests.Integration.Helpers;

public abstract class IntegrationTestBase : IAsyncLifetime, IDisposable
{
    private readonly BookstoreWebApplicationFactory _factory = new();

    protected HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        // CreateClient starts the application
        Client = _factory.CreateClient();
        await AuthenticateAsAdminAsync();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        // The Dispose(bool) is left empty since all real cleanup is in DisposeAsync()
    }

    protected async Task AuthenticateAsAdminAsync()
    {
        await AuthenticateAsync("admin", "Admin123!");
    }

    protected async Task AuthenticateAsReaderAsync()
    {
        await AuthenticateAsync("reader", "Reader123!");
    }

    protected void ClearAuthentication()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    protected async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body = null)
    {
        using var request = new HttpRequestMessage(method, new Uri($"/api/v1/{path}", UriKind.Relative));
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await Client.SendAsync(request);
    }

    protected async Task<TResponse> PostAsync<TResponse>(string path, object body)
    {
        using var response = await Client.PostAsJsonAsync(new Uri($"/api/v1/{path}", UriKind.Relative), body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TResponse>())!;
    }

    protected async Task<TResponse> GetAsync<TResponse>(string path)
    {
        using var response = await Client.GetAsync(new Uri($"/api/v1/{path}", UriKind.Relative));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TResponse>())!;
    }

    protected async Task<IList<TResponse>> GetListAsync<TResponse>(string path)
    {
        using var response = await Client.GetAsync(new Uri($"/api/v1/{path}", UriKind.Relative));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<IList<TResponse>>())!;
    }

    private async Task AuthenticateAsync(string username, string password)
    {
        var loginRequest = new LoginRequest { Username = username, Password = password };
        using var response = await Client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResponse!.Token);
    }
}
