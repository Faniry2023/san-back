namespace SAN_API.Services
{
    public class KoboAssetDetails
    {
        public string? Uid { get; set; }
        public string? Name { get; set; }
        public string? AssetType { get; set; }

        // Children présent seulement quand on appelle /assets/{uid}/ (pour un projet)
        public List<KoboAsset>? Children { get; set; }
    }
}
