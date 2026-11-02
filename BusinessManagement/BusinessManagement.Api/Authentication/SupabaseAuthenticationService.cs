using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace BusinessManagement.Api.Authentication;

public sealed class SupabaseAuthenticationService(
    HttpClient httpClient,
    IConfiguration configuration)
{
    public async Task<SupabaseTokenResponse?> SignInWithPasswordAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        string? anonKey = configuration["Authentication:Supabase:AnonKey"];
        if (string.IsNullOrWhiteSpace(anonKey))
        {
            throw new InvalidOperationException("Authentication:Supabase:AnonKey is not configured.");
        }

        using HttpRequestMessage request = new(HttpMethod.Post, "auth/v1/token?grant_type=password")
        {
            Content = JsonContent.Create(new
            {
                email,
                password
            })
        };
        request.Headers.Add("apikey", anonKey);

        using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<SupabaseTokenResponse>(cancellationToken);
    }
}

public sealed record SupabaseTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("expires_at")] long? ExpiresAt,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("user")] SupabaseUserResponse? User);

public sealed record SupabaseUserResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("email")] string? Email);
