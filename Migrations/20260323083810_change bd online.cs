using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAN_API.Migrations
{
    /// <inheritdoc />
    public partial class changebdonline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiKeyKobo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id_utilisateur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Key = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeyKobo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Authorise",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id_utilisateur = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Graphique = table.Column<bool>(type: "bit", nullable: false),
                    Situation = table.Column<bool>(type: "bit", nullable: false),
                    Vulnerabilite = table.Column<bool>(type: "bit", nullable: false),
                    Carte = table.Column<bool>(type: "bit", nullable: false),
                    Utilisateur = table.Column<bool>(type: "bit", nullable: false),
                    Acces = table.Column<bool>(type: "bit", nullable: false),
                    Kobo = table.Column<bool>(type: "bit", nullable: false),
                    PowerBi = table.Column<bool>(type: "bit", nullable: false),
                    Gadm = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authorise", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Disponibilites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id_prod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Id_temps = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disponibilites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Enquetes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enquetes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Es",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Prenom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Id_gadm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nb_site = table.Column<int>(type: "int", nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Es", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Evenements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id_enquete = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Id_val_mens = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cle_principal = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evenements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormulairesKobo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Uid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nom_formulaire = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Not_del = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormulairesKobo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Gadm",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    NomLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gid = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gadm", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupeKobo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id_formulaire = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Uid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nom_groupe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nom_technique = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Is_reapet = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupeKobo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Login",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Login", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prosuits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id_temps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Id_gadm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Id_enquete = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Id_es = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nom_prod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Unite = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cle = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prosuits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionKobo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id_groupe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionKobo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Relations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id_evenement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Id_val_mensuel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Id_temps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReponseKobo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id_soumission = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Id_question = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reponse = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReponseKobo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Soumissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Uid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date_soumission = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Soumissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Temps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom_evenement = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Annee = table.Column<int>(type: "int", nullable: true),
                    Jan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fev = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Avr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Jui = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Juill = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aou = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sep = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Oct = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nov = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dec = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Temps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    Id_login = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Matricule = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Prenom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Photo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gadm = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Val_mensuels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id_temps = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Id_gadm = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nom_val = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Id_es = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Val_mensuels", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKeyKobo");

            migrationBuilder.DropTable(
                name: "Authorise");

            migrationBuilder.DropTable(
                name: "Disponibilites");

            migrationBuilder.DropTable(
                name: "Enquetes");

            migrationBuilder.DropTable(
                name: "Es");

            migrationBuilder.DropTable(
                name: "Evenements");

            migrationBuilder.DropTable(
                name: "FormulairesKobo");

            migrationBuilder.DropTable(
                name: "Gadm");

            migrationBuilder.DropTable(
                name: "GroupeKobo");

            migrationBuilder.DropTable(
                name: "Login");

            migrationBuilder.DropTable(
                name: "Prosuits");

            migrationBuilder.DropTable(
                name: "QuestionKobo");

            migrationBuilder.DropTable(
                name: "Relations");

            migrationBuilder.DropTable(
                name: "ReponseKobo");

            migrationBuilder.DropTable(
                name: "Soumissions");

            migrationBuilder.DropTable(
                name: "Temps");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "Val_mensuels");
        }
    }
}
