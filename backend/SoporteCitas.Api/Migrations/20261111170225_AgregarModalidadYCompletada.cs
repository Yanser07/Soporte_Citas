using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoporteCitas.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarModalidadYCompletada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Citas",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCompletada",
                table: "Citas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Modalidad",
                table: "Citas",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<string>(
                name: "NotasInternas",
                table: "Citas",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResumenTrabajo",
                table: "Citas",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "FechaCompletada",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "Modalidad",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "NotasInternas",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "ResumenTrabajo",
                table: "Citas");
        }
    }
}
