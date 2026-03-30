using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SAN_API.Data;
using SAN_API.Helper;
using System.Threading.Tasks;

namespace SAN_API.Controllers
{
    public class GadmController : Controller
    {
        private readonly DataContext dataContext;
        public GadmController(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }
        [HttpGet("gadm/getGadm")]
        public async Task<IActionResult> GetGadm()
        {
            if (dataContext is null && dataContext?.Gadm is null)
            {
                return Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Erreur serveur",
                        detail: "Le context de données est introuvable"
                );
            }
            var allGadm = await dataContext.Gadm.ToListAsync();
            if (allGadm is null || allGadm.Count < 1)
            {
                return Problem(
                       statusCode: StatusCodes.Status404NotFound,
                       title: "Gadm introuvable",
                       detail: "Aucun Gadm trouvée dans la base de données"
                   );
            }
            GadmHelper gadmHelper = new();
            gadmHelper.Provinces = allGadm.Where(g => g.Level == 1).ToList();
            gadmHelper.Regions = allGadm.Where(g => g.Level == 2).ToList();
            gadmHelper.Districts = allGadm.Where(g => g.Level == 3).ToList();
            gadmHelper.Communes = allGadm.Where(g => g.Level == 4).ToList();

            return Ok(gadmHelper);
        }
    }
}
