namespace SAN_API.Models
{
    public class ESModel
    {
        public Guid Id { get; set; }
        public string? Nom {  get; set; }
        public string? Prenom {  get; set; }
        public string? Id_gadm {  get; set; }
        public int Nb_site {  get; set; }
        public string? Telephone {  get; set; }
    }
}
