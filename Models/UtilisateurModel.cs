namespace SAN_API.Models
{
    public class UtilisateurModel
    {
        public Guid Id { get; set; }
        public string? Id_login {  get; set; }
        public string? Matricule {  get; set; }
        public string? Nom {  get; set; }
        public string? Prenom {  get; set; }
        public string? Contact {  get; set; }
        public string? Email { get; set; }
        public string? Photo {  get; set; }
    }
}
