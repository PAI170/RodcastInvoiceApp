using System.Net.Http.Json;
using System.Text.Json;

namespace RodcastInvoiceApp.Web.Security
{
    // Valida el token de Cloudflare Turnstile (el widget del login) contra la
    // API de Cloudflare antes de siquiera intentar el login con email/password.
    public interface ITurnstileVerifier
    {
        Task<bool> VerifyAsync(string? token, string? remoteIp);
    }

    public class TurnstileVerifier : ITurnstileVerifier
    {
        private const string VerifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

        private readonly HttpClient _httpClient;
        private readonly string _secretKey;

        public TurnstileVerifier(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _secretKey = configuration["Turnstile:SecretKey"] ?? string.Empty;
        }

        public async Task<bool> VerifyAsync(string? token, string? remoteIp)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(_secretKey))
                return false;

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["secret"] = _secretKey,
                ["response"] = token,
                ["remoteip"] = remoteIp ?? string.Empty
            });

            var response = await _httpClient.PostAsync(VerifyUrl, content);
            if (!response.IsSuccessStatusCode)
                return false;

            var result = await response.Content.ReadFromJsonAsync<TurnstileResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result?.Success ?? false;
        }

        private class TurnstileResponse
        {
            public bool Success { get; set; }
        }
    }
}
