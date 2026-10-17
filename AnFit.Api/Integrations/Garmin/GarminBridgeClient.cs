using AnFit.Api.Extensions;
using AnFit.Api.Exceptions;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AnFit.Api.Integrations.Garmin
{
    /// <summary>
    /// Client gọi tới Python Garmin Bridge Microservice thay vì gọi trực tiếp Garmin API.
    /// Thư viện Python (garminconnect) sẽ xử lý việc đăng nhập và lấy dữ liệu.
    /// 
    /// Khi nào có Garmin Developer API key chính thức, chỉ cần đổi lại 1 dòng DI
    /// trong Program.cs từ GarminBridgeClient sang GarminClient là xong.
    /// </summary>
    public class GarminBridgeClient : IGarminClient
    {
        private readonly HttpClient _httpClient;
        private readonly GarminSettings _settings;

        public GarminBridgeClient(HttpClient httpClient, IOptions<GarminSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;

            // Cấu hình base URL trỏ tới Python FastAPI service
            _httpClient.BaseAddress = new Uri(_settings.BridgeBaseUrl);
        }

        /// <summary>
        /// Không cần OAuth khi dùng Bridge mode.
        /// Python service tự đăng nhập bằng email/password.
        /// </summary>
        public Task<(string OAuthToken, string OAuthTokenSecret)> GetRequestTokenAsync()
        {
            // Bridge mode: không cần OAuth token dance
            // Trả về chuỗi đánh dấu "BRIDGE" để GarminSyncService biết đang dùng Bridge
            return Task.FromResult(("BRIDGE_MODE", "BRIDGE_MODE"));
        }

        /// <summary>
        /// Không cần redirect URL khi dùng Bridge mode.
        /// </summary>
        public string GetAuthorizeUrl(string requestToken, string callbackUrl)
        {
            return "BRIDGE_MODE_NO_REDIRECT_NEEDED";
        }

        /// <summary>
        /// Không cần exchange token khi dùng Bridge mode.
        /// </summary>
        public Task<(string AccessToken, string AccessTokenSecret)> ExchangeAccessTokenAsync(
            string requestToken, string requestTokenSecret, string oauthVerifier)
        {
            return Task.FromResult(("BRIDGE_CONNECTED", "BRIDGE_CONNECTED"));
        }

        /// <summary>
        /// Gọi Python Garmin Bridge để lấy dữ liệu sức khỏe thực từ Garmin Cloud.
        /// Trả về JSON string giống format của Garmin Health API chính thức.
        /// </summary>
        public async Task<string> GetDailySummaryAsync(string accessToken, string accessTokenSecret, DateOnly date)
        {
            // Giải mã mật khẩu từ Base64 (nếu có lưu)
            var email = accessToken;
            var password = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(accessTokenSecret))
                {
                    var base64Bytes = Convert.FromBase64String(accessTokenSecret);
                    password = System.Text.Encoding.UTF8.GetString(base64Bytes);
                }
            }
            catch
            {
                // Fallback nếu không phải Base64
                password = accessTokenSecret;
            }

            // Gọi tới Python FastAPI: GET http://localhost:8001/api/health/daily/2026-04-13
            var dateString = date.ToString("yyyy-MM-dd");
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/health/daily/{dateString}");
            
            if (!string.IsNullOrEmpty(email))
            {
                request.Headers.Add("X-Garmin-Email", email);
            }
            if (!string.IsNullOrEmpty(password))
            {
                request.Headers.Add("X-Garmin-Password", password);
            }

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                throw new BadRequestException($"Không thể lấy dữ liệu từ Garmin Bridge: {errorMsg}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Kiểm tra đăng nhập Garmin Bridge bằng tài khoản người dùng truyền từ Flutter.
        /// </summary>
        public async Task<bool> ValidateCredentialsAsync(string email, string password)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/health/status");
            if (!string.IsNullOrEmpty(email))
            {
                request.Headers.Add("X-Garmin-Email", email);
            }
            if (!string.IsNullOrEmpty(password))
            {
                request.Headers.Add("X-Garmin-Password", password);
            }

            try
            {
                var response = await _httpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
