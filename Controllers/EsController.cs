using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAN_API.Data;
using SAN_API.Helper;
using SAN_API.Models;
using System.Threading.Tasks;

namespace SAN_API.Controllers
{
    public class EsController : Controller
    {
        private readonly DataContext dataContext;

        public EsController(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }
        [HttpGet("es/getall")]
        public async Task<IActionResult> GetAll()
        {
            if (dataContext is null && dataContext?.Es is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }
            var liste_es = await dataContext.Es.ToListAsync();
            if(liste_es is null)
            {
                return Problem(
                       statusCode: StatusCodes.Status404NotFound,
                       title: "ES introuvable",
                       detail: "Aucun Es trouvée dans la base de données"
                   );
            }

            return Ok(liste_es);
        }

        [HttpGet("es/getbyid/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (dataContext is null && dataContext?.Es is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }
            var es = await dataContext.Es.FirstOrDefaultAsync(e => e.Id.ToString().ToUpper().Equals(id.ToUpper()));
            if(es is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: "Es introuvable",
                        detail: "Aucun ES trouvée dans la base de données"
                    );
            }
            return Ok(es);
        }
        [HttpPost("es/create")]
        public async Task<IActionResult> Create([FromBody] EsHelper model)
        {
            if (model is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Requete invalide",
                    detail: "La requête est invalide ou manquant"
                );
            }
            if (dataContext is null && dataContext?.Es is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }
            ESModel newEs = new();
            newEs.Nom = model.Nom;
            newEs.Prenom = model.Prenom;
            newEs.Id_gadm = model.Id_gadm!.ToUpper();
            newEs.Nb_site = model.Nb_site;
            newEs.Telephone = model.Telephone;

            await dataContext.Es.AddAsync(newEs);
            await dataContext.SaveChangesAsync();
            return Ok(newEs);
        }
    }
}
