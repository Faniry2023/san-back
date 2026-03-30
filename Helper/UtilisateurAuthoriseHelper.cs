using SAN_API.Models;

namespace SAN_API.Helper
{
    public class UtilisateurAuthoriseHelper
    {
        public UtilisateurModel? Utilisateur {  get; set; }
        public AuthoriseModel? Authorise { get; set; }
        public byte[]? Photo {  get; set; }
    }
}
