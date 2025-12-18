namespace SAN_API.Services.KoboToolBox.ModelKoboTollbox
{
    public class GroupeInfoModel
    {
        public bool IsRepeat { get; set; }
        public string? Nom { get; set; }
        public string? Nom_technique { get; set; }
        public List<string>? Questions { get; set; }
    }
}
