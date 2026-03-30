using SAN_API.Models;

namespace SAN_API.Helper
{
    public class ReponseApiKoboHelper
    {
        public int NbrSoummission {  get; set; }
        public int Tailler {  get; set; }
        public DateTime Date_soumission { get; set; }
        public Dictionary<string, List<ReponseKoboModel>> Reponse_dico { get; set; } = new();
    }
}
