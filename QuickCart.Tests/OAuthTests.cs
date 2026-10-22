using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using QuickCart.Utility;
using Xunit;

namespace QuickCart.Tests
{
    public class OAuthTests
    {
        [Fact]
        public void OAuthSettings_DefaultsToEmptyStrings()
        {
            var settings = new OAuthSettings();

            Assert.Equal(string.Empty, settings.GoogleClientId);
            Assert.Equal(string.Empty, settings.GoogleClientSecret);
            Assert.Equal(string.Empty, settings.GoogleCallbackPath);
        }

        [Fact]
        public void GetAuthorizationUrl_ForGoogle_ConstructsValidEndpointWithScopes()
        {
            var settings = Options.Create(new OAuthSettings
            {
                GoogleClientId = "google-test-id",
                GoogleClientSecret = "google-secret"
            });
            var mockFactory = new Mock<IHttpClientFactory>();
            var mockLogger = new Mock<ILogger<OAuthService>>();
            var service = new OAuthService(mockFactory.Object, settings, mockLogger.Object);

            var url = service.GetAuthorizationUrl("google", "https://app.com/callback", "xyz123");

            Assert.Contains("https://accounts.google.com/o/oauth2/v2/auth", url);
            Assert.Contains("client_id=google-test-id", url);
            Assert.Contains("scope=openid%20profile%20email", url);
            Assert.Contains("state=xyz123", url);
        }

        [Fact]
        public void GetAuthorizationUrl_WhenProviderUnsupported_ThrowsNotSupportedException()
        {
            var settings = Options.Create(new OAuthSettings());
            var mockFactory = new Mock<IHttpClientFactory>();
            var mockLogger = new Mock<ILogger<OAuthService>>();
            var service = new OAuthService(mockFactory.Object, settings, mockLogger.Object);

            Assert.Throws<NotSupportedException>(() => service.GetAuthorizationUrl("github", "https://app.com", "state"));
            Assert.Throws<NotSupportedException>(() => service.GetAuthorizationUrl("twitter", "https://app.com", "state"));
        }

        [Fact]
        public void GetAuthorizationUrl_WhenProviderEmpty_ThrowsArgumentException()
        {
            var settings = Options.Create(new OAuthSettings());
            var mockFactory = new Mock<IHttpClientFactory>();
            var mockLogger = new Mock<ILogger<OAuthService>>();
            var service = new OAuthService(mockFactory.Object, settings, mockLogger.Object);

            Assert.Throws<ArgumentException>(() => service.GetAuthorizationUrl("", "https://app.com", "state"));
        }

        [Theory]
        [InlineData("", "code", "https://redirect")]
        [InlineData("google", "", "https://redirect")]
        [InlineData("google", null, "https://redirect")]
        [InlineData("unknown", "validcode", "https://redirect")]
        public async Task ExchangeCodeForUserInfoAsync_WhenParametersInvalid_ReturnsNullTuple(string provider, string? code, string redirect)
        {
            var settings = Options.Create(new OAuthSettings());
            var mockFactory = new Mock<IHttpClientFactory>();
            var mockLogger = new Mock<ILogger<OAuthService>>();
            var service = new OAuthService(mockFactory.Object, settings, mockLogger.Object);

            var (email, name) = await service.ExchangeCodeForUserInfoAsync(provider, code!, redirect);

            Assert.Null(email);
            Assert.Null(name);
        }

        [Theory]
        [InlineData(null, "~/")]
        [InlineData("", "~/")]
        [InlineData("   ", "~/")]
        [InlineData("https://evil.com", "~/")]
        [InlineData("//evil.com", "~/")]
        [InlineData("/\\evil.com", "~/")]
        [InlineData("javascript:alert(1)", "~/")]
        [InlineData("/Identity/Account/Login", "~/")]
        [InlineData("/Identity/Account/Login?ReturnUrl=%2F", "~/")]
        [InlineData("/Identity/Account/ExternalLogin", "~/")]
        [InlineData("/Identity/Account/ExternalLogin?handler=Callback", "~/")]
        [InlineData("/Identity/Account/ExternalLogin?provider=Google", "~/")]
        [InlineData("/Identity/Account/Register", "~/")]
        [InlineData("/Customer/Home/Index", "/Customer/Home/Index")]
        [InlineData("/Customer/Cart/Summary", "/Customer/Cart/Summary")]
        [InlineData("/Admin/Order/Details?orderId=5", "/Admin/Order/Details?orderId=5")]
        [InlineData("~/Customer/Home/Details?productId=1", "~/Customer/Home/Details?productId=1")]
        public void CleanReturnUrl_SanitizesMaliciousAndSelfReferentialLoops(string? input, string expected)
        {
            var result = SD.CleanReturnUrl(input);

            Assert.Equal(expected, result);
        }
    }
}
