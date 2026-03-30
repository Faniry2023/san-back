using SAN_API.Models;

namespace SAN_API.Helper.Vulnerabilite
{
    public class VulnerabiliteHelper
    {
        public EnqueteModel? Enquete { get; set; }
        public EvenementModel? Evenement { get; set; }
        public Val_mensuelModel? Val_mensuel { get; set; }
        public RelationModel? Relation { get; set; }
        public ESModel? Es {  get; set; }
        public GadmModel? Gadm { get; set; }
        public TempsModel? Temps {  get; set; }
    }
}
