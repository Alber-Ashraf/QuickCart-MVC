using System.Threading.Tasks;

namespace QuickCart.Utility
{
    public interface IOAuthService
    {
        string GetAuthorizationUrl(string provider, string redirectUri, string state);
        Task<(string? Email, string? FullName)> ExchangeCodeForUserInfoAsync(string provider, string code, string redirectUri);
    }
}
