using Microsoft.AspNetCore.Mvc;
using SAN_API.Services;
using System.Threading.Tasks;

namespace SAN_API.Controllers
{
    public class TesteController : Controller
    {
        [HttpGet("recDonne")]
        public async Task<IActionResult> Index()
        {
            string id = "agDQ2Bhz5ojHeMx5TmHyQq";
            var json = await KoboApi.GetFromData(id);
            return Content(json, "application/json");
            //return Ok(json);
        }
        [HttpGet("recFormulaire")]
        public async Task<IActionResult> Formulaire()
        {
            var json = await KoboApi.GetForms();
            //return Ok(json);
            return Content(json, "application/json");
        }

        [HttpGet("mesProjets")]
        public async Task<IActionResult> MesProjets()
        {
            var json = await KoboApi.GetAllProjetsAsync();
            return Content($"{json}", "application/json");
        }

    }
}
