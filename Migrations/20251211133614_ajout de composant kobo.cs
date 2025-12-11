using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAN_API.Migrations
{
    /// <inheritdoc />
    public partial class ajoutdecomposantkobo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FormulairesKobo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nom_formulaire = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormulairesKobo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupeKobo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id_formulaire = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nom_groupe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Is_reapet = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupeKobo", x => x.Id);
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
                name: "ReponseKobo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id_question = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reponse = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReponseKobo", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormulairesKobo");

            migrationBuilder.DropTable(
                name: "GroupeKobo");

            migrationBuilder.DropTable(
                name: "QuestionKobo");

            migrationBuilder.DropTable(
                name: "ReponseKobo");
        }
    }
}
