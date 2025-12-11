using System.ComponentModel.DataAnnotations;

namespace SAN_API.Models
{
    public class GadmModel
    {
        [Key]
        public string? Id {  get; set; }
        public int Level { get; set; }
        public string? NomLevel {  get; set; }
        public string? Nom {  get; set; }
        public string? Gid {  get; set; }
    }
}
