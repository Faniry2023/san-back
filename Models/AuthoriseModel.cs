namespace SAN_API.Models
{
    public class AuthoriseModel
    {
        public Guid Id { get; set; }
        public string? Id_utilisateur {  get; set; }
        public bool Graphique {  get; set; }
        public bool Situation {  get; set; }
        public bool Vulnerabilite {  get; set; }
        public bool Carte {  get; set; }
        public bool Utilisateur { get; set; }
        public bool Acces {  get; set; }
        public bool Kobo {  get; set; }
        public bool PowerBi {  get; set; }

        public string? Gadm {  get; set; }
    }
}
