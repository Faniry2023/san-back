using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SAN_API.Migrations
{
    /// <inheritdoc />
    public partial class ameliorationmodelkobo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Date_soumission",
                table: "FormulairesKobo",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Uid",
                table: "FormulairesKobo",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date_soumission",
                table: "FormulairesKobo");

            migrationBuilder.DropColumn(
                name: "Uid",
                table: "FormulairesKobo");
        }
    }
}
