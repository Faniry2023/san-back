using SAN_API.Models;
using SAN_API.Services.KoboToolBox.ModelKoboTollbox;

namespace SAN_API.Helper
{
    public class RetoureKoboFormulaireHelper
    {
        public bool IsLess {  get; set; }
        public bool IsMore {  get; set; }
        public List<KoboAssetModel>? koboAssets { get; set; } = new();
        public List<FormulaireKoboModel>? koboForm { get; set; } = new();
        public int CountNew {  get; set; }
        public bool Alert {  get; set; }
    }
}
