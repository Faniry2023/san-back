using SAN_API.Models;

namespace SAN_API.Helper
{
    public class CompletSituationHelper
    {
        public ProduitModel? Produit {  get; set; }
        public TempsModel? Temps {  get; set; }
        public ESModel? Es { get; set; }
        public EnqueteModel? Enquete { get; set; }
        public GadmModel? Gadm {  get; set; }
        public DisponibiliteHelper? Disponibilite { get; set; }
    }
}
