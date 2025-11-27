using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAN_API.Migrations
{
    /// <inheritdoc />
    public partial class ajoutdeid_logindansutilisateur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Id_login",
                table: "Utilisateurs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id_login",
                table: "Utilisateurs");
        }
    }
}
