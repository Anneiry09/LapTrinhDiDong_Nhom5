namespace AnFit.Api.DTOs.Requests
{
    public class GarminCallbackRequest
    {
        public string OAuthToken { get; set; } = string.Empty;
        public string OAuthVerifier { get; set; } = string.Empty;
    }
}