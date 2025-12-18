using Microsoft.EntityFrameworkCore;
using SAN_API.Models;

namespace SAN_API.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<LoginModel> Login { get; set; }
        public DbSet<UtilisateurModel> Utilisateurs { get; set; }
        public DbSet<GadmModel> Gadm {  get; set; }
        public DbSet<FormulaireKoboModel> FormulairesKobo { get; set; }
        public DbSet<GroupeKoboModel> GroupeKobo { get;set; }
        public DbSet<QuestionKoboModel> QuestionKobo { get; set ;}
        public DbSet<ReponseKoboModel> ReponseKobo {  get; set; }
        public DbSet<KoboSoumissionModel> Soumissions {  get; set; }
    }
}
