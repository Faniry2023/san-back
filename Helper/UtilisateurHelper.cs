using System.Text.Json.Serialization;

namespace SAN_API.Helper
{
    public class UtilisateurHelper
    {
        [JsonPropertyName("matricule")]
        public string? Matricule { get; set; }
        [JsonPropertyName("nom")]
        public string? Nom { get; set; }
        [JsonPropertyName("prenom")]
        public string? Prenom { get; set; }
        [JsonPropertyName("contact")]
        public string? Contact { get; set; }
    }
}
