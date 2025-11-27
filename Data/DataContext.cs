using Microsoft.EntityFrameworkCore;
using SAN_API.Models;

namespace SAN_API.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<LoginModel> Login { get; set; }
        public DbSet<UtilisateurModel> Utilisateurs { get; set; }
    }
}
