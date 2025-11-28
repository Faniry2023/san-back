using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAN_API.Data;
using SAN_API.Helper;
using SAN_API.Models;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace SAN_API.Controllers
{
    public class AuthentificationController : Controller
    {
        private  DataContext dataContext;
        private JwtHelper jwtHelper;
        public AuthentificationController(DataContext dataContext, JwtHelper jwtHelper)
        {
            this.dataContext = dataContext;
            this.jwtHelper = jwtHelper;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]SeConLoginHelper model)
        {
            if (model is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Requete invalide",
                    detail: "La requête est invalide ou manquant"
                );
            }
            if (dataContext?.Utilisateurs is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }
            var login = await dataContext.Login.FirstOrDefaultAsync(l => l.Email!.Equals(model.Identifier) || l.Username!.Equals(model.Identifier));
            if(login is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Utilisateur introuvable",
                        detail: "Aucun utilisateur ne correspond à cet email/username"
                    );
            }
            bool loginOk = CryptageHelper.VerifyPassword(model.Password!,login.Password!);

            if (!loginOk)
            {
                return Problem(
                        statusCode: StatusCodes.Status401Unauthorized,
                        title: "Echec de l'authentification",
                        detail: "Mot de passe incorrecte"
                    );
            }
            var utilisateur = await dataContext.Utilisateurs.FirstOrDefaultAsync(u => u.Id_login!.ToUpper().Equals(login.Id.ToString()!.ToUpper()));
            if(utilisateur is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Utilisateur introuvable",
                        detail: "Une erreur lors de votre authentification"
                    );
            }
            var token = jwtHelper.GenerateToken(utilisateur.Id, login.Email);
            int heure = (model.Remember) ? 24 : 1;
            Response.Cookies.Append("jwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(heure)
            });
            return Ok(utilisateur);
        }

        [Authorize]
        [HttpPost("sigin")]
        public async Task<IActionResult> CreateUser([FromForm]IFormCollection modelForm)
        {
            if (modelForm is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Requete invalide",
                    detail: "La requête est invalide ou manquant"
                );
            }
            var loginJson = modelForm["login"];
            var utilisateurJson = modelForm["utilisateur"];
            var photoJson = modelForm.Files["photo"];

            var loginModel = JsonSerializer.Deserialize<LoginHelper>(loginJson!);
            var utilisateurModel = JsonSerializer.Deserialize<UtilisateurHelper>(utilisateurJson!);

            if (dataContext?.Login is null || dataContext?.Utilisateurs is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }
            GenerateFileType generate = new();
            var pathPhoto = generate.SaveFile(photoJson!, "Images");
            LoginModel login = new();
            UtilisateurModel utilisateur = new();
            login.Username = loginModel!.Username;
            login.Email = loginModel!.Email;
            login.Password = CryptageHelper.HashPassword(loginModel.Password!);

            await dataContext.Login.AddAsync(login);
            await dataContext.SaveChangesAsync();

            utilisateur.Id_login = login.Id.ToString().ToUpper();
            utilisateur.Matricule = utilisateurModel!.Matricule;
            utilisateur.Nom = utilisateurModel!.Nom;
            utilisateur.Prenom = utilisateurModel!.Prenom;
            utilisateur.Email = loginModel!.Email;
            utilisateur.Contact = utilisateurModel.Contact;
            utilisateur.Photo = pathPhoto;

            await dataContext.Utilisateurs.AddAsync(utilisateur);
            await dataContext.SaveChangesAsync();

            return Ok(utilisateur);
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out Guid id))
            {
                return Problem(
                        statusCode: StatusCodes.Status401Unauthorized,
                        title: "User Id invalid",
                        detail: "Id de l'utilisateur non valide"
                    );
            }
            if (dataContext?.Utilisateurs == null)
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );

            var utilisateru = await dataContext.Utilisateurs.FirstOrDefaultAsync(u =>
                                u.Id.ToString().ToUpper().Equals(userId.ToUpper()));
            if (utilisateru == null)
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Utilisateur introuvable",
                        detail: "Aucun utilisateur trouvée"
                    );

            return Ok(utilisateru);
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {

            //suppression
            Response.Cookies.Delete("jwtToken");
            return Ok(new { message = "Déconnexion réussie" });
        }

        [
    }
}
