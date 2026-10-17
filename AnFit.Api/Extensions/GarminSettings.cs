namespace AnFit.Api.Extensions
{
    public class GarminSettings
    {
        public string ConsumerKey { get; set; } = string.Empty;
        public string ConsumerSecret { get; set; } = string.Empty;
        public string RequestTokenUrl { get; set; } = string.Empty;
        public string AccessTokenUrl { get; set; } = string.Empty;
        public string AuthorizeUrl { get; set; } = string.Empty;

        // URL của Python Garmin Bridge Microservice
        public string BridgeBaseUrl { get; set; } = "http://localhost:8001";
    }
}