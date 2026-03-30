using SAN_API.Models;

namespace SAN_API.Helper
{
    public class ReponseForViewHelper
    {
        public FormulaireKoboModel Formulaire {  get; set; }
        public List<ReponseApiKoboHelper> Reponse_soumi { get; set; } = new();
        public Dictionary<string, List<QuestionKoboModel>> Questions_dico { get; set; } = new();
    }
}
