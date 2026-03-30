using SAN_API.Models;

namespace SAN_API.Helper
{
    public class GadmHelper
    {
        public List<GadmModel> Provinces { get; set; } = new();
        public List<GadmModel> Regions { get; set; } = new();
        public List<GadmModel> Districts { get; set; } = new();
        public List<GadmModel> Communes { get; set; } = new();
    }
}
