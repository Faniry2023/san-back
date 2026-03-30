using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        private readonly DataContext dataContext;
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
            if (dataContext is null && dataContext?.Utilisateurs is null && dataContext?.Authorise is null)
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
            var authorise = await dataContext.Authorise.FirstOrDefaultAsync(a => a.Id_utilisateur.ToUpper().Equals(utilisateur.Id.ToString().ToUpper()));
            if (authorise is null && !utilisateur.IsAdmin)
            {
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Utilisateur introuvable",
                        detail: "Une erreur lors de votre authentification"
                    );
            }
            UtilisateurAuthoriseHelper uah = new();
            if(utilisateur.Photo != null)
            {
                GenerateFileType gft = new();
                uah.Photo = gft.OutputFile(utilisateur.Photo);
            }
            uah.Utilisateur = utilisateur;
            uah.Authorise = utilisateur.IsAdmin ? new AuthoriseModel() : authorise;
            var token = jwtHelper.GenerateToken(utilisateur.Id, login.Email);
            int heure = (model.Remember) ? 24 : 1;
            Response.Cookies.Append("jwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(heure)
            });
            return Ok(uah);
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
            utilisateur.IsAdmin = utilisateurModel!.IsAdmin;
            utilisateur.Matricule = utilisateurModel!.Matricule;
            utilisateur.Nom = utilisateurModel!.Nom;
            utilisateur.Prenom = utilisateurModel!.Prenom;
            utilisateur.Email = loginModel!.Email;
            utilisateur.Contact = utilisateurModel.Contact;
            utilisateur.Photo = pathPhoto;
            utilisateur.Gadm = utilisateurModel.Gadm;

            await dataContext.Utilisateurs.AddAsync(utilisateur);

            AuthoriseModel am = new();
            if (!utilisateur.IsAdmin)
            {
                am.Id_utilisateur = utilisateur.Id.ToString().ToUpper();
                am.Graphique = false;
                am.Situation = false;
                am.Vulnerabilite = false;
                am.Carte = false;
                am.Utilisateur = false;
                am.Acces = false;
                am.Kobo = false;
                am.PowerBi = false;
                am.Gadm = "aucun";
                await dataContext.Authorise.AddAsync(am);
            }
            await dataContext.SaveChangesAsync();
            UtilisateurAuthoriseHelper uah = new();
            if(utilisateur.Photo != null)
            {
                GenerateFileType gft = new();
                uah.Photo = gft.OutputFile(utilisateur.Photo);
            }
            uah.Utilisateur = utilisateur;
            uah.Authorise = am;

            return Ok(uah);
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
            if (dataContext == null && dataContext?.Utilisateurs == null && dataContext?.Authorise == null)
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );

            var utilisateru = await dataContext.Utilisateurs.FirstOrDefaultAsync(u =>
                                u.Id.ToString().ToUpper().Equals(userId.ToUpper()));
            if (utilisateru == null)
                return Problem(
                        statusCode: StatusCodes.Status401Unauthorized,
                        title: "Utilisateur introuvable",
                        detail: "Aucun utilisateur trouvée"
                    );
            
            var authorize = await dataContext.Authorise.FirstOrDefaultAsync(a => a.Id_utilisateur.ToUpper().Equals(utilisateru.Id.ToString().ToUpper()));
            if (authorize == null && !utilisateru.IsAdmin)
                return Problem(
                        statusCode: StatusCodes.Status401Unauthorized,
                        title: "Utilisateur introuvable",
                        detail: "Aucun utilisateur trouvée"
                    );
            UtilisateurAuthoriseHelper uah = new();
            if (utilisateru.Photo != null)
            {
                GenerateFileType gft = new();
                uah.Photo = gft.OutputFile(utilisateru.Photo);
            }
            uah.Utilisateur = utilisateru;
            uah.Authorise = utilisateru.IsAdmin ? new AuthoriseModel() : authorize;
            return Ok(uah);
        }

        [Authorize]
        [HttpGet("getalluser")]
        public async Task<IActionResult> GetAllUser()
        {
            if (dataContext == null && dataContext?.Utilisateurs == null && dataContext?.Authorise == null)
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            var all = await dataContext.Utilisateurs.ToListAsync();
            if(all == null)
            {
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Aucun donnée",
                        detail: "Utilisateur null"
                );
            }
            List<UtilisateurAuthoriseHelper> liste_utilisateur = new();
            foreach(var item in all)
            {
                UtilisateurAuthoriseHelper uah = new();
                if (!item.IsAdmin)
                {
                    var authorise = await dataContext.Authorise.FirstOrDefaultAsync(a => a.Id_utilisateur!.ToUpper().Equals(item.Id.ToString().ToUpper()));
                    if(authorise == null)
                    {
                        return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Aucun donnée",
                        detail: "Utilisateur null (authorise null)"
                        );
                    }
                    uah.Authorise = authorise;
                }
                else
                {
                    uah.Authorise = new AuthoriseModel();
                }
                GenerateFileType gft = new();
                if(item.Photo == null || item.Photo.IsNullOrEmpty())
                {
                    return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Aucune photo",
                        detail: "photo utilisateur null"
                    );
                }
                uah.Photo = gft.OutputFile(item.Photo);
                uah.Utilisateur = item;
                liste_utilisateur.Add(uah);
            }
            return Ok(liste_utilisateur);
        }

        [Authorize]
        [HttpPost("addauthorise")]
        public async Task<IActionResult> AddAuthorise([FromBody] AuthoriseHelper model)
        {
            if (dataContext == null && dataContext?.Utilisateurs == null && dataContext?.Authorise == null)
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            var authorise = await dataContext.Authorise.FirstOrDefaultAsync(a => a.Id.ToString().ToUpper().Equals(model.Id!.ToUpper()));
            if(authorise == null)
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Authorise Erreur",
                        detail: "authorisation introuvable"
                );
            authorise.Graphique = model.Graphique;
            authorise.Situation = model.Situation;
            authorise.Vulnerabilite = model.Vulnerabilite;
            authorise.Carte = model.Carte;
            authorise.Utilisateur = model.Utilisateur;
            authorise.Acces = model.Acces;
            authorise.Kobo = model.Kobo;
            authorise.PowerBi = model.PowerBi;
            authorise.Gadm = model.Gadm;

            await dataContext.SaveChangesAsync();

            return Ok(authorise);
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {

            //suppression
            Response.Cookies.Delete("jwtToken");
            return Ok(new { message = "Déconnexion réussie" });
        }
        //[Authorize]
        //[HttpPost("insert/gadm")]
        //public async Task<IActionResult> InsertGadm([FromBody] GadmModel model)
        //{
        //    if(model is not null)
        //    {
        //        if(dataContext?.Gadm is not null)
        //        {
        //            GadmModel gadm = new();
        //            gadm = model;
        //            await dataContext.Gadm.AddAsync(gadm);
        //            await dataContext.SaveChangesAsync();
        //            return Ok(new { message = "Enregistrement ok" });
        //        }
        //        return StatusCode(500, "Erreur serveur");
        //    }
        //    return BadRequest("Requette invalide");
        //}
    }
}
