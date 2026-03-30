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
        public DbSet<ProduitModel> Prosuits {  get; set; }
        public DbSet<DisponibiliteModel> Disponibilites { get; set; }
        public DbSet<TempsModel> Temps { get; set; }
        public DbSet<ESModel> Es { get; set; }
        public DbSet<EnqueteModel> Enquetes { get; set; }
        public DbSet<RelationModel> Relations {  get; set; }
        public DbSet<EvenementModel> Evenements { get; set; }
        public DbSet<Val_mensuelModel> Val_mensuels { get; set; }
        public DbSet<AuthoriseModel> Authorise {  get; set; }
        public DbSet<ApiKeyKoboModel> ApiKeyKobo { get; set; }

    }
}
