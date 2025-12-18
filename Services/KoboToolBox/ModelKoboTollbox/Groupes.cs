
namespace SAN_API.Services.KoboToolBox.ModelKoboTollbox
{
    public class Groupes
    {
        public string? Nom_technique { get; set; }
        public List<ReponseParQuestion> Reponse { get; set; } = new();
    }
}
