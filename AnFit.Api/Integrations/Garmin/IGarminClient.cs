using System;
using System.Threading.Tasks;

namespace AnFit.Api.Integrations.Garmin
{
    public interface IGarminClient
    {
        // 1. Lấy Token tạm thời để user bấm đồng ý
        Task<(string OAuthToken, string OAuthTokenSecret)> GetRequestTokenAsync();

        // 2. Tạo URL để frontend mở popup cho user đăng nhập Garmin
        string GetAuthorizeUrl(string requestToken, string callbackUrl);

        // 3. Đổi Token tạm lấy Token chính thức (sau khi user đồng ý)
        Task<(string AccessToken, string AccessTokenSecret)> ExchangeAccessTokenAsync(string requestToken, string requestTokenSecret, string oauthVerifier);

        // 4. Lấy dữ liệu sức khỏe (Calo, Bước chân...)
        Task<string> GetDailySummaryAsync(string accessToken, string accessTokenSecret, DateOnly date);

        // 5. Kiểm tra thông tin đăng nhập Garmin Bridge
        Task<bool> ValidateCredentialsAsync(string email, string password);
    }
}