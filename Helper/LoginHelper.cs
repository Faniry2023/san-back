using System.Text.Json.Serialization;

namespace SAN_API.Helper
{
    public class LoginHelper
    {
        [JsonPropertyName("id")]
        public string? Id {  get; set; }
        [JsonPropertyName("username")]
        public string? Username { get; set; }
        [JsonPropertyName("email")]
        public string? Email { get; set; }
        [JsonPropertyName("password")]
        public string? Password { get; set; }
        [JsonPropertyName("remember")]
        public bool Remember { get; set; }
    }
}
