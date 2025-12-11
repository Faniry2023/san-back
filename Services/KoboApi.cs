using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SAN_API.Services
{
    public class KoboApi
    {
        private const string ApiKey = "79919da5ece5312529f96d0e32894e44d66525dc";
        private const string BaseUrl = "https://kf.kobotoolbox.org/api/v2/assets/";

        //recupere un formulaire par son id
        public static async Task<string> GetFromData(string assetId)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", $"{ApiKey}");

            var url = $"{BaseUrl}{assetId}/data/";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<string> GetForms()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", $"{ApiKey}");

            var url = $"{BaseUrl}?asset_type=form";

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<List<string>> GetAllProjetsAsync()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", $"{ApiKey}");
            var url = $"{BaseUrl}?asset_type=form";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();


            var json = await response.Content.ReadAsStringAsync();
            // Désérialiser
            var data = JsonConvert.DeserializeObject<KoboAssetList>(json);
            //return await response.Content.ReadAsStringAsync();
            return data.Results.Select(r => r.Uid).ToList();
        }
        public static async Task<List<string>> GetFormLabelsAsync(string formId)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Token", ApiKey);

            var url = $"{BaseUrl}{formId}/";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var form = JsonConvert.DeserializeObject<KoboFormDetails>(json);

            List<string> labels = new();

            string[] nonQuestionTypes = {
                "begin_group", "end_group", "group",
                "begin_repeat", "end_repeat", "repeat"
            };

            foreach (var q in form.Content.Survey)
            {
                // Ignorer groupes / repeat
                if (q.Type != null && nonQuestionTypes.Contains(q.Type))
                    continue;

                if (q.Label == null) continue;

                var token = JToken.FromObject(q.Label);

                if (token.Type == JTokenType.String)
                {
                    labels.Add(token.ToString());
                }
                else if (token.Type == JTokenType.Object)     // multilingue
                {
                    var obj = token.ToObject<Dictionary<string, string>>();
                    if (obj.ContainsKey("fr"))
                        labels.Add(obj["fr"]);
                    else
                        labels.Add(obj.Values.First());
                }
                else if (token.Type == JTokenType.Array)      // array
                {
                    var arr = token.ToObject<List<string>>();
                    labels.Add(arr.First());
                }
            }

            return labels;
        }




        //separation question par groupe
        public static async Task<Dictionary<string, List<string>>> GetFormGroupeStructureAsync(string formId)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Token", ApiKey);

            var url = $"{BaseUrl}{formId}/";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var form = JsonConvert.DeserializeObject<KoboFormDetails>(json);

            // Résultat final
            Dictionary<string, List<string>> structure = new();

            string currentGroup = "Racine"; // questions non groupées

            structure[currentGroup] = new List<string>();

            foreach (var q in form.Content.Survey)
            {
                if (q.Type == "begin_group" || q.Type == "begin_repeat")
                {
                    // Label du groupe
                    string groupLabel = ExtractLabel(q.Label);

                    currentGroup = groupLabel;
                    structure[currentGroup] = new List<string>();
                    continue;
                }

                if (q.Type == "end_group" || q.Type == "end_repeat")
                {
                    currentGroup = "Racine";
                    continue;
                }

                // Ce n'est pas une question → skip
                if (IsGroupType(q.Type)) continue;

                string label = ExtractLabel(q.Label);
                if (label != null)
                    structure[currentGroup].Add(label);
            }
            foreach (var key in structure.Keys.ToList())
            {
                if (structure[key].Count == 0)
                    structure.Remove(key);
            }

            return structure;
        }
        static bool IsGroupType(string type)
        {
            if (type == null) return false;

            string[] nonQuestions = {
                "begin_group", "end_group",
                "group",
                "begin_repeat", "end_repeat", "repeat"
            };

            return nonQuestions.Contains(type);
        }
        static string ExtractLabel(object label)
        {
            if (label == null) return null;

            var token = JToken.FromObject(label);

            if (token.Type == JTokenType.String)
                return token.ToString();

            if (token.Type == JTokenType.Object)
            {
                var dict = token.ToObject<Dictionary<string, string>>();
                if (dict.ContainsKey("fr"))
                    return dict["fr"];
                return dict.Values.FirstOrDefault();
            }

            if (token.Type == JTokenType.Array)
            {
                var arr = token.ToObject<List<string>>();
                return arr.FirstOrDefault();
            }

            return null;
        }




    }
}
