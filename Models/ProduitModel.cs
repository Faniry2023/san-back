namespace SAN_API.Models
{
    public class ProduitModel
    {
        public Guid Id { get; set; }
        public string? Id_temps {  get; set; }
        public string? Id_gadm {  get; set; }
        public string? Id_enquete {  get; set; }
        public string? Nom_prod {  get; set; }
        public string? Unite {  get; set; }
        public string? Cle {  get; set; }
    }
}
