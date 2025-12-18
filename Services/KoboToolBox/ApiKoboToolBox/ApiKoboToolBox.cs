using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SAN_API.Services.KoboToolBox.ModelKoboTollbox;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SAN_API.Services.KoboToolBox.ApiKoboToolBox
{
    public class ApiKoboToolBox
    {
        private const string ApiKey = "cle_api_kobotoolbox_a_remplacer";
        private const string BaseUrl = "https://kf.kobotoolbox.org/api/v2/assets/";

        public static RetoureModel  AuthorizationMethod(string addUrl)
        {
            //authentification sur l'api kobotoolbox
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", $"{ApiKey}");
            RetoureModel retour = new();
            retour.Url = $"{BaseUrl}{addUrl}";
            retour.Client = client;
            return retour;
        }

        //Récupération de tout les projets
        public static async Task<List<KoboAssetModel>> GetAllProject()
        {
            //lien vers l'api pour la récupération de tout les formulaires
            var retour = AuthorizationMethod("?asset_type=form");

            //récupération de la valeur de l'api
            var response = await retour.Client!.GetAsync(retour.Url);
            response.EnsureSuccessStatusCode();

            //Désérialisation du reponse pour être inserer dans le model
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<KoboAssetList>(json);
            return data!.Results!;
        }

        //recuperer tout les composant dur formulaire
        public static async Task<KoboFormDetailsModel> GetFormAsync(string id_formulaire)
        {
            var retoure = AuthorizationMethod(id_formulaire+"/");

            var response = await retoure.Client!.GetAsync(retoure.Url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<KoboFormDetailsModel>(json)!;
        }

        // Extrait un label propre (réutilise la version précédente)
        private static List<string> FlattenArray(JToken token)
        {
            var list = new List<string>();
            if (token == null) return list;

            if (token.Type == JTokenType.Array)
            {
                foreach (var child in token.Children())
                {
                    if (child.Type == JTokenType.String)
                    {
                        var s = child.ToString();
                        if (!string.IsNullOrWhiteSpace(s)) list.Add(s);
                    }
                    else if (child.Type == JTokenType.Array || child.Type == JTokenType.Object)
                    {
                        list.AddRange(FlattenArray(child));
                    }
                    else if (child.Type == JTokenType.Integer || child.Type == JTokenType.Float)
                    {
                        list.Add(child.ToString());
                    }
                }
            }
            else if (token.Type == JTokenType.String)
            {
                var s = token.ToString();
                if (!string.IsNullOrWhiteSpace(s)) list.Add(s);
            }

            return list;
        }
        private static string? ExtractLabelRaw(KoboQuestionModel q)
        {
            if (q?.Label == null) return null;

            if (q.Label is string s)
            {
                if (!string.IsNullOrWhiteSpace(s)) return s.Trim();
                return null;
            }

            var token = JToken.FromObject(q.Label);

            if (token.Type == JTokenType.String)
            {
                var val = token.ToString();
                return string.IsNullOrWhiteSpace(val) ? null : val.Trim();
            }

            if (token.Type == JTokenType.Object)
            {
                var dict = token.ToObject<Dictionary<string, string>>();
                if (dict != null && dict.Count > 0)
                {
                    if (dict.ContainsKey("fr") && !string.IsNullOrWhiteSpace(dict["fr"])) return dict["fr"].Trim();
                    var first = dict.Values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
                    return first?.Trim();
                }
                return null;
            }

            if (token.Type == JTokenType.Array)
            {
                var flat = FlattenArray(token);
                var first = flat.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                return first?.Trim();
            }

            return null;
        }

        // Version corrigée de l'extracteur
        public static Dictionary<string, GroupeInfoModel> ExtractGroupsClean(KoboFormDetailsModel form)
        {
            var results = new Dictionary<string, GroupeInfoModel>();
            if (form?.Content?.Survey == null) return results;

            // types structurels à ignorer (non-questions)
            var ignoreTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "start", "end",
                    "note", "calculate",
                    "trigger", "meta", "label"
                };

            string currentGroup = "Racine";
            results[currentGroup] = new GroupeInfoModel { IsRepeat = false, Nom = "Racine", Questions = new List<string>() };
            foreach (var q in form.Content.Survey)
            {
                var type = (q.Type ?? string.Empty).ToLowerInvariant();

                // Ignorer complètement certains types structurels
                if (ignoreTypes.Contains(type)) continue;
                if (type == "begin_group" || type == "begin_repeat")
                {
                    var rawLabel = ExtractLabelRaw(q);

                    string nomLisible = !string.IsNullOrWhiteSpace(rawLabel)
                        ? rawLabel
                        : q.Name ?? "Groupe_SansNom";

                    string nomTechnique = q.Name ?? "Groupe_SansNom";

                    // clé unique pour le dictionnaire (basée sur le label)
                    var key = nomLisible;
                    int suffix = 1;
                    while (results.ContainsKey(key))
                    {
                        key = $"{nomLisible} ({++suffix})";
                    }

                    results[key] = new GroupeInfoModel
                    {
                        IsRepeat = (type == "begin_repeat"),
                        Nom = nomLisible,
                        Nom_technique = nomTechnique,
                        Questions = new List<string>()
                    };

                    currentGroup = key;
                    continue;
                }
                // End group / end repeat -> revenir à Racine
                if (type == "end_group" || type == "end_repeat")
                {
                    currentGroup = "Racine";
                    continue;
                }

                // Autres types considérés comme questions (text, integer, select_one, etc.)
                // Récupérer un label propre si possible, sinon fallback sur name.
                var label = ExtractLabelRaw(q) ?? q.Name;

                // si label est toujours null/empty -> on skip (évite 'start'/'end' ou items vides)
                if (string.IsNullOrWhiteSpace(label)) continue;

                // ajouter la question au groupe courant
                results[currentGroup].Questions.Add(label);
            }

            // supprimer les groupes vides si tu veux
            var keys = results.Keys.ToList();
            foreach (var k in keys)
            {
                if (results[k].Questions == null || results[k].Questions.Count == 0)
                    results.Remove(k);
            }

            return results;
        }

        //nettoyage des reponses
        public static async Task<JArray> Cleaner(string id_formulaire)
        {
            string url = $"{id_formulaire}/data/?format=json";
            var retoure = AuthorizationMethod(url);
            var json = await retoure.Client!.GetStringAsync(retoure.Url);

            var root = JObject.Parse(json);
            return (JArray)root["results"]!;
        }

        //Un reponse nettoyer
        public static ReponseComplet MapOneRecord(JObject record)
        {
            var result = new ReponseComplet();

            foreach (var prop in record.Properties())
            {
                // ignorer métadonnées Kobo
                if (prop.Name.StartsWith("_") || prop.Name.Contains("/"))
                    continue;

                // 🔹 GROUPE RÉPÉTÉ
                if (prop.Value.Type == JTokenType.Array)
                {
                    var group = new Groupes
                    {
                        Nom_technique = prop.Name
                    };

                    foreach (JObject item in prop.Value)
                    {
                        var rep = new ReponseParQuestion();

                        foreach (var q in item.Properties())
                        {
                            rep.ReponseQuestion.Add(q.Value?.ToString());
                        }

                        group.Reponse.Add(rep);
                    }

                    result.GroupeResponse.Add(group);
                }
                // 🔹 QUESTION SIMPLE
                else if (prop.Value.Type != JTokenType.Object)
                {
                    result.Reponse.Add(prop.Value?.ToString());
                }
            }

            return result;
        }

        //reponses nettoyer
        public static async Task<List<ReponseComplet>> AllAnswer(string id_formulaire)
        {
            var results = await Cleaner(id_formulaire);
            var cleaned = results
                .Select(r => MapOneRecord((JObject)r))
                .ToList();
            return cleaned;
        }

    }
}
