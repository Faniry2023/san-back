using SAN_API.Models;

namespace SAN_API.Helper.Vulnerabilite
{
    public class PostVulnerabiliteHelper
    {
        public EnqueteVulHelper? Enquete {  get; set; }
        public EveVulHelper? Evenement {  get; set; }
        public ValVulHelper? Val_mensuel { get; set; }
        public RelVulHelper? Relation {  get; set; }
        public EsHelper? Es {  get; set; }
        public GadmModel? Gadm {get; set; }  
        public TempsHelper? Temps { get; set; }
    }
}
