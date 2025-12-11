using Microsoft.AspNetCore.Mvc;
using SAN_API.Services;
using System.Threading.Tasks;

namespace SAN_API.Controllers
{
    public class TesteController : Controller
    {
        //[HttpGet("formulaire")]
        //public async Task<IActionResult> Index(string id)
        //{
        //    //string id = "agDQ2Bhz5ojHeMx5TmHyQq";
        //    //var json = await KoboApi.GetFromData(id);
        //    //return Content(json, "application/json");
            //return Ok(json);
            //var liste = await KoboApi.FormulaireProjetsAsync(id);
        //    return Ok(liste);
        //}
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
            var liste = await KoboApi.GetAllProjetsAsync();
            return Ok(liste);
        }
        [HttpGet("mesQuestion/{id}")]
        public async Task<IActionResult> Question(string id)
        {
            var liste = await KoboApi.GetFormLabelsAsync(id);
            return Ok(liste);
        }
        [HttpGet("QuestionByGroupe/{id}")]
        public async Task<IActionResult> QuestionByGroupe(string id)
        {
            var structure = await KoboApi.GetFormGroupeStructureAsync(id);
            return Ok(structure);
        }

    }
}
