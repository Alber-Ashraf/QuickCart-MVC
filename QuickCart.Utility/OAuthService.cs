using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QuickCart.Utility
{
    public class OAuthService : IOAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly OAuthSettings _oauthSettings;
        private readonly ILogger<OAuthService> _logger;

        public OAuthService(
            IHttpClientFactory httpClientFactory,
            IOptions<OAuthSettings> oauthSettings,
            ILogger<OAuthService> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _oauthSettings = oauthSettings?.Value ?? new OAuthSettings();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string GetAuthorizationUrl(string provider, string redirectUri, string state)
        {
            if (string.IsNullOrWhiteSpace(provider))
                throw new ArgumentException("Provider cannot be empty.", nameof(provider));

            if (provider.Equals("google", StringComparison.OrdinalIgnoreCase))
            {
                var clientId = _oauthSettings.GoogleClientId;
                return $"https://accounts.google.com/o/oauth2/v2/auth?client_id={HttpUtility.UrlEncode(clientId)}&redirect_uri={HttpUtility.UrlEncode(redirectUri)}&response_type=code&scope=openid%20profile%20email&state={HttpUtility.UrlEncode(state)}";
            }

            throw new NotSupportedException($"Unsupported OAuth provider: '{provider}'");
        }

        public async Task<(string? Email, string? FullName)> ExchangeCodeForUserInfoAsync(string provider, string code, string redirectUri)
        {
            if (string.IsNullOrWhiteSpace(provider)) return (null, null);
            if (string.IsNullOrWhiteSpace(code)) return (null, null);

            try
            {
                using var httpClient = _httpClientFactory.CreateClient();

                if (provider.Equals("Google", StringComparison.OrdinalIgnoreCase))
                {
                    var tokenRequest = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("code", code),
                        new KeyValuePair<string, string>("client_id", _oauthSettings.GoogleClientId),
                        new KeyValuePair<string, string>("client_secret", _oauthSettings.GoogleClientSecret),
                        new KeyValuePair<string, string>("redirect_uri", redirectUri),
                        new KeyValuePair<string, string>("grant_type", "authorization_code")
                    });

                    var tokenResponse = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest);
                    if (!tokenResponse.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Google OAuth token exchange failed with status {StatusCode}", tokenResponse.StatusCode);
                        return (null, null);
                    }

                    using var tokenDoc = await JsonDocument.ParseAsync(await tokenResponse.Content.ReadAsStreamAsync());
                    if (!tokenDoc.RootElement.TryGetProperty("id_token", out var idTokenProp))
                    {
                        return (null, null);
                    }

                    var idToken = idTokenProp.GetString();
                    var userResponse = await httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
                    if (!userResponse.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Google OAuth tokeninfo call failed with status {StatusCode}", userResponse.StatusCode);
                        return (null, null);
                    }

                    using var userDoc = await JsonDocument.ParseAsync(await userResponse.Content.ReadAsStreamAsync());
                    string? email = userDoc.RootElement.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
                    string? name = userDoc.RootElement.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;

                    return (email, name);
                }

                return (null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ExchangeCodeForUserInfoAsync failed for provider {Provider}", provider);
                return (null, null);
            }
        }
    }
}
