using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAN_API.Data;
using SAN_API.Helper;
using SAN_API.Models;
using System.Threading.Tasks;

namespace SAN_API.Controllers
{
    public class EnqueteController : Controller
    {
        private readonly DataContext dataContext;
        public EnqueteController(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        [HttpGet("enquete/all")]
        public async Task<IActionResult> GetAll()
        {
            if (dataContext is null && dataContext?.Enquetes is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }
            var list_enquete = await dataContext.Enquetes.ToListAsync();
            if (list_enquete is null)
            {
                return Problem(
                       statusCode: StatusCodes.Status404NotFound,
                       title: "Enquetes introuvable",
                       detail: "Aucun enquête trouvée dans la base de données"
                   );
            }

            return Ok(list_enquete);
        }
        [HttpPost("enquete/add")]
        public async Task<IActionResult> Add([FromBody] EnqueteHelper model)
        {
            if (model is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Requete invalide",
                    detail: "La requête est invalide ou manquant"
                );
            }
            if (dataContext is null && dataContext?.Enquetes is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }
            EnqueteModel enquete = new();
            enquete.Nom = model.Nom;

            await dataContext.Enquetes.AddAsync(enquete);
            await dataContext.SaveChangesAsync();
            return Ok(enquete);
        }

    }
}
