using AnFit.Api.Extensions;
using Microsoft.Extensions.Options;
using OAuth;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace AnFit.Api.Integrations.Garmin
{
    public class GarminClient : IGarminClient
    {
        private readonly HttpClient _httpClient;
        private readonly GarminSettings _settings;

        public GarminClient(HttpClient httpClient, IOptions<GarminSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<(string OAuthToken, string OAuthTokenSecret)> GetRequestTokenAsync()
        {
            var oauthRequest = OAuthRequest.ForRequestToken(_settings.ConsumerKey, _settings.ConsumerSecret, OAuthSignatureMethod.HmacSha1);
            oauthRequest.RequestUrl = _settings.RequestTokenUrl;
            oauthRequest.Method = "POST";
            
            // Garmin often requires a callback, but according to previous it was omitted here or implicitly handled.
            // If callback is needed, oauthRequest.CallbackUrl can be set.

            var authHeader = oauthRequest.GetAuthorizationHeader();

            var request = new HttpRequestMessage(HttpMethod.Post, _settings.RequestTokenUrl);
            request.Headers.Add("Authorization", authHeader);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var parsedParams = HttpUtility.ParseQueryString(responseContent);

            return (parsedParams["oauth_token"] ?? "", parsedParams["oauth_token_secret"] ?? "");
        }

        public string GetAuthorizeUrl(string requestToken, string callbackUrl)
        {
            return $"{_settings.AuthorizeUrl}?oauth_token={requestToken}&oauth_callback={HttpUtility.UrlEncode(callbackUrl)}";
        }

        public async Task<(string AccessToken, string AccessTokenSecret)> ExchangeAccessTokenAsync(string requestToken, string requestTokenSecret, string oauthVerifier)
        {
            var oauthRequest = OAuthRequest.ForAccessToken(_settings.ConsumerKey, _settings.ConsumerSecret, requestToken, requestTokenSecret, oauthVerifier, OAuthSignatureMethod.HmacSha1);
            oauthRequest.RequestUrl = _settings.AccessTokenUrl;
            oauthRequest.Method = "POST";

            var authHeader = oauthRequest.GetAuthorizationHeader();

            var request = new HttpRequestMessage(HttpMethod.Post, _settings.AccessTokenUrl);
            request.Headers.Add("Authorization", authHeader);

            var content = new StringContent($"oauth_verifier={oauthVerifier}", System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var parsedParams = HttpUtility.ParseQueryString(responseContent);

            return (parsedParams["oauth_token"] ?? "", parsedParams["oauth_token_secret"] ?? "");
        }

        public async Task<string> GetDailySummaryAsync(string accessToken, string accessTokenSecret, DateOnly date)
        {
            var dateTime = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var timestamp = ((DateTimeOffset)dateTime).ToUnixTimeSeconds();

            string url = $"https://healthapi.garmin.com/wellness-api/rest/dailies?uploadStartTimeInSeconds={timestamp}&uploadEndTimeInSeconds={timestamp + 86400}";

            var oauthRequest = OAuthRequest.ForProtectedResource("GET", _settings.ConsumerKey, _settings.ConsumerSecret, accessToken, accessTokenSecret, OAuthSignatureMethod.HmacSha1);
            oauthRequest.RequestUrl = url;

            var authHeader = oauthRequest.GetAuthorizationHeader();

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", authHeader);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return string.Empty;

            return await response.Content.ReadAsStringAsync();
        }

        public Task<bool> ValidateCredentialsAsync(string email, string password)
        {
            // Official Garmin client doesn't need bridge credential validation
            return Task.FromResult(true);
        }
    }
}