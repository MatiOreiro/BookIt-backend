using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookIt.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedTagsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "FechaCreacion", "Nombre" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111101"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "DJ" },
                    { new Guid("11111111-1111-1111-1111-111111111102"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Música en vivo" },
                    { new Guid("11111111-1111-1111-1111-111111111103"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Catering" },
                    { new Guid("11111111-1111-1111-1111-111111111104"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Comida" },
                    { new Guid("11111111-1111-1111-1111-111111111105"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Decoración" },
                    { new Guid("11111111-1111-1111-1111-111111111106"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fotografía" },
                    { new Guid("11111111-1111-1111-1111-111111111107"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Videografía" },
                    { new Guid("11111111-1111-1111-1111-111111111108"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Salón de eventos" },
                    { new Guid("11111111-1111-1111-1111-111111111109"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Floristería" },
                    { new Guid("11111111-1111-1111-1111-111111111110"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pastelería" },
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Animación" },
                    { new Guid("11111111-1111-1111-1111-111111111112"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Maquillaje" },
                    { new Guid("11111111-1111-1111-1111-111111111113"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Peluquería" },
                    { new Guid("11111111-1111-1111-1111-111111111114"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Invitaciones" },
                    { new Guid("11111111-1111-1111-1111-111111111115"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Transporte" },
                    { new Guid("11111111-1111-1111-1111-111111111116"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Iluminación" },
                    { new Guid("11111111-1111-1111-1111-111111111117"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sonido" },
                    { new Guid("11111111-1111-1111-1111-111111111118"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Renta de equipos" },
                    { new Guid("11111111-1111-1111-1111-111111111119"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Celebrante" },
                    { new Guid("11111111-1111-1111-1111-111111111120"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Coordinación de eventos" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Tags",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    new Guid("11111111-1111-1111-1111-111111111101"),
                    new Guid("11111111-1111-1111-1111-111111111102"),
                    new Guid("11111111-1111-1111-1111-111111111103"),
                    new Guid("11111111-1111-1111-1111-111111111104"),
                    new Guid("11111111-1111-1111-1111-111111111105"),
                    new Guid("11111111-1111-1111-1111-111111111106"),
                    new Guid("11111111-1111-1111-1111-111111111107"),
                    new Guid("11111111-1111-1111-1111-111111111108"),
                    new Guid("11111111-1111-1111-1111-111111111109"),
                    new Guid("11111111-1111-1111-1111-111111111110"),
                    new Guid("11111111-1111-1111-1111-111111111111"),
                    new Guid("11111111-1111-1111-1111-111111111112"),
                    new Guid("11111111-1111-1111-1111-111111111113"),
                    new Guid("11111111-1111-1111-1111-111111111114"),
                    new Guid("11111111-1111-1111-1111-111111111115"),
                    new Guid("11111111-1111-1111-1111-111111111116"),
                    new Guid("11111111-1111-1111-1111-111111111117"),
                    new Guid("11111111-1111-1111-1111-111111111118"),
                    new Guid("11111111-1111-1111-1111-111111111119"),
                    new Guid("11111111-1111-1111-1111-111111111120")
                });
        }
    }
}

