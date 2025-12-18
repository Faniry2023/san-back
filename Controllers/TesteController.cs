using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SAN_API.Services;
using SAN_API.Services.KoboToolBox.ApiKoboToolBox;
using SAN_API.Services.KoboToolBox.ModelKoboTollbox;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SAN_API.Controllers
{
    public class Test
    {
        public List<string> Questions { get; set; }
        public List<string> Reponse {  get; set; }
    }
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
        [HttpGet("projets")]
        public async Task<IActionResult> Projets()
        {
            /*
            var nom_projets = await KoboApi.GetAllProjetsAsync();
            List<KoboAsset> ass = new();
            ass = await KoboApi.GetAllProjetsAsyncAll();
            */
            List<KoboAssetModel> liste = new();
            liste = await ApiKoboToolBox.GetAllProject();
            return Ok(liste);
        }
        [HttpGet("Survey/{id}")]
        public async Task<IActionResult> Survey(string id)
        {
            var surveys = await ApiKoboToolBox.GetFormAsync(id);
            //return Ok(surveys);
            //var form = await KoboApi.GetFormAsync(id);
            // log raw json to file (dev only)
            System.IO.File.WriteAllText("debug_kobo_raw.json", JsonConvert.SerializeObject(surveys, Formatting.Indented));
            var cleaned = ApiKoboToolBox.ExtractGroupsClean(surveys);
            return Ok(cleaned);
        }
        [HttpGet("Kobobrute/{id}")]
        public async Task<IActionResult> KoboApiBrute(string id)
        {
            //string BaseUrl = "https://kf.kobotoolbox.org/api/v2/assets/";
            var client = ApiKoboToolBox.AuthorizationMethod(id + "/data/?format=json&limit=1000&start=0");


            var response = await client.Client!.GetAsync(client.Url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            // retourne exactement ce que Kobo a envoyé
            return Content(json, "application/json");
        }
        [HttpGet("clean/{id}")]
        public async Task<IActionResult> Clean(string id)
        {
            /*
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Token", "79919da5ece5312529f96d0e32894e44d66525dc");

            var url = "https://kf.kobotoolbox.org/api/v2/assets/aJNuwBRxpG23BNRrA4tkLG/data/?format=json";
            var json = await client.GetStringAsync(url);

            var root = JObject.Parse(json);
            var results = (JArray)root["results"];

            var cleaned = results
                .Select(r => KoboResponseMapper.MapOneRecord((JObject)r))
                .ToList();*/
            var cleaned = await ApiKoboToolBox.AllAnswer(id);

            return Ok(cleaned);
        }

        [HttpGet("jsontolist")]
        public IActionResult ListToJson()
        {
            var questions = new List<string> { "nom", "prenom", "age", "adresse" };
            var reponses = new List<string> { "Horolé", "David", "45", "Paris" };

            // Vérifier que les deux listes ont la même longueur
            if (questions.Count != reponses.Count)
                throw new Exception("Les listes n'ont pas la même taille !");

            // Créer un dictionnaire qui mappe question -> réponse
            var dict = new Dictionary<string, string>();
            for (int i = 0; i < questions.Count; i++)
            {
                dict[questions[i]] = reponses[i];
            }

            // Convertir en JSON
            string json = JsonConvert.SerializeObject(dict, Formatting.Indented);


            var dictFromJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            // Recréer les listes
            var questionsFromJson = new List<string>(dictFromJson.Keys);
            var reponsesFromJson = new List<string>(dictFromJson.Values);

            Test test = new Test();
            test.Questions = questionsFromJson;
            test.Reponse = reponsesFromJson;
            return Ok(test);
        }

        //[HttpGet("groupe")]
        //public IActionResult Groupe()
        //{
        //    var url = "https://kf.kobotoolbox.org/api/v2/assets/aJNuwBRxpG23BNRrA4tkLG/";
        //    foreach (var q in form.Content.Survey)
        //    {
        //        if (q.Type == "begin_repeat")
        //        {
        //            string groupName = q.Name;      // nom technique
        //            string groupLabel = ExtractLabel(q.Label); // nom lisible (ex: "informations")
        //        }
        //    }

        //}
        //[HttpGet("valeur/{id}")]
        //public async Task<IActionResult> Valeur(string id)
        //{
        //    var form = await KoboApi.GetFormAsync(id);
        //    var data = await KoboDataHelper.GetFormResponsesAsync(id);

        //    var structure = KoboFormParser.ExtractStructure(form);
        //    var cleaned = KoboResponseCleaner.CleanResponses(data, structure);

        //    return Ok(cleaned);
        //}
        private static string? GetSingleValue(JToken? token)
        {
            if (token == null) return null;

            if (token.Type == JTokenType.Array)
            {
                var first = token.FirstOrDefault();
                if (first == null) return null;

                // Si le premier élément est encore un tableau imbriqué (ex: [[]])
                if (first.Type == JTokenType.Array)
                {
                    var inner = first.FirstOrDefault();
                    return inner?.ToString();
                }

                return first.ToString();
            }

            // Sinon, c’est une valeur simple
            return token.ToString();
        }


    }
}
