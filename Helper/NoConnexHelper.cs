using SAN_API.Models;

namespace SAN_API.Helper
{
    public class NoConnexHelper
    {
        public bool Isconnect {  get; set; }
        public List<FormulaireKoboModel> Liste_formulaire { get; set; } = new();
    }
}
