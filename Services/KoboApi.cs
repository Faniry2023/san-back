using System.Net.Http.Headers;

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

        public static async Task<string> GetAllProjetsAsync()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", $"{ApiKey}");
            var url = $"{BaseUrl}?asset_type=form";
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
