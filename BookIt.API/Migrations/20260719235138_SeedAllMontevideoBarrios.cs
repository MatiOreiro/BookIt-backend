using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookIt.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedAllMontevideoBarrios : Migration
    {
        // The renames in Up/Down permute names among existing Montevideo barrio rows
        // (e.g. row 1 "Centro" -> "Ciudad Vieja", the name row 3 currently holds).
        // Renaming in-place would trip the unique (DepartamentoId, Nombre) index
        // mid-transaction, so every changing row is first staged through a placeholder
        // that can't collide with anything, then the real renames are applied.
        private static readonly string[] MontevideoRenameIds =
        {
            "20000000-0000-0000-0000-000000000001",
            "20000000-0000-0000-0000-000000000002",
            "20000000-0000-0000-0000-000000000003",
            "20000000-0000-0000-0000-000000000004",
            "20000000-0000-0000-0000-000000000005",
            "20000000-0000-0000-0000-000000000006",
            "20000000-0000-0000-0000-000000000008",
            "20000000-0000-0000-0000-000000000010",
            "20000000-0000-0000-0000-000000000011",
            "20000000-0000-0000-0000-000000000012",
            "20000000-0000-0000-0000-000000000013",
            "20000000-0000-0000-0000-000000000014",
            "20000000-0000-0000-0000-000000000015",
            "20000000-0000-0000-0000-000000000016",
            "20000000-0000-0000-0000-000000000017",
            "20000000-0000-0000-0000-000000000018",
            "20000000-0000-0000-0000-000000000019",
            "20000000-0000-0000-0000-000000000020",
            "20000000-0000-0000-0000-000000000021",
            "20000000-0000-0000-0000-000000000022",
            "20000000-0000-0000-0000-000000000023",
            "20000000-0000-0000-0000-000000000024",
            "20000000-0000-0000-0000-000000000025"
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var id in MontevideoRenameIds)
            {
                migrationBuilder.UpdateData(
                    table: "Barrios",
                    keyColumn: "Id",
                    keyValue: new Guid(id),
                    column: "Nombre",
                    value: $"__tmp_migrating_{id[^4..]}");
            }

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "Nombre",
                value: "Ciudad Vieja");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "Nombre",
                value: "Centro");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                column: "Nombre",
                value: "Barrio Sur");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                column: "Nombre",
                value: "Cordón");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                column: "Nombre",
                value: "Palermo");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000006"),
                column: "Nombre",
                value: "Parque Rodó");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000008"),
                column: "Nombre",
                value: "Pocitos");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000010"),
                column: "Nombre",
                value: "La Unión");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000011"),
                column: "Nombre",
                value: "La Blanqueada");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000012"),
                column: "Nombre",
                value: "Parque Batlle");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000013"),
                column: "Nombre",
                value: "Villa Dolores");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000014"),
                column: "Nombre",
                value: "La Mondiola");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000015"),
                column: "Nombre",
                value: "Malvín");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000016"),
                column: "Nombre",
                value: "Malvín Norte");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000017"),
                column: "Nombre",
                value: "Punta Gorda");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000018"),
                column: "Nombre",
                value: "Carrasco");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000019"),
                column: "Nombre",
                value: "Carrasco Norte");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000020"),
                column: "Nombre",
                value: "Tres Cruces");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000021"),
                column: "Nombre",
                value: "La Comercial");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000022"),
                column: "Nombre",
                value: "Villa Muñoz");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000023"),
                column: "Nombre",
                value: "Goes");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000024"),
                column: "Nombre",
                value: "Aguada");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000025"),
                column: "Nombre",
                value: "Reducto");

            migrationBuilder.InsertData(
                table: "Barrios",
                columns: new[] { "Id", "DepartamentoId", "Nombre" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000026"), new Guid("10000000-0000-0000-0000-000000000010"), "Arroyo Seco" },
                    { new Guid("20000000-0000-0000-0000-000000000027"), new Guid("10000000-0000-0000-0000-000000000010"), "Bella Vista" },
                    { new Guid("20000000-0000-0000-0000-000000000028"), new Guid("10000000-0000-0000-0000-000000000010"), "La Figurita" },
                    { new Guid("20000000-0000-0000-0000-000000000029"), new Guid("10000000-0000-0000-0000-000000000010"), "Jacinto Vera" },
                    { new Guid("20000000-0000-0000-0000-000000000030"), new Guid("10000000-0000-0000-0000-000000000010"), "Larrañaga" },
                    { new Guid("20000000-0000-0000-0000-000000000031"), new Guid("10000000-0000-0000-0000-000000000010"), "Maroñas" },
                    { new Guid("20000000-0000-0000-0000-000000000032"), new Guid("10000000-0000-0000-0000-000000000010"), "Parque Guaraní" },
                    { new Guid("20000000-0000-0000-0000-000000000033"), new Guid("10000000-0000-0000-0000-000000000010"), "Flor de Maroñas" },
                    { new Guid("20000000-0000-0000-0000-000000000034"), new Guid("10000000-0000-0000-0000-000000000010"), "Villa Española" },
                    { new Guid("20000000-0000-0000-0000-000000000035"), new Guid("10000000-0000-0000-0000-000000000010"), "Simón Bolívar" },
                    { new Guid("20000000-0000-0000-0000-000000000036"), new Guid("10000000-0000-0000-0000-000000000010"), "Brazo Oriental" },
                    { new Guid("20000000-0000-0000-0000-000000000037"), new Guid("10000000-0000-0000-0000-000000000010"), "Atahualpa" },
                    { new Guid("20000000-0000-0000-0000-000000000038"), new Guid("10000000-0000-0000-0000-000000000010"), "Prado" },
                    { new Guid("20000000-0000-0000-0000-000000000039"), new Guid("10000000-0000-0000-0000-000000000010"), "Capurro" },
                    { new Guid("20000000-0000-0000-0000-000000000040"), new Guid("10000000-0000-0000-0000-000000000010"), "Paso Molino" },
                    { new Guid("20000000-0000-0000-0000-000000000041"), new Guid("10000000-0000-0000-0000-000000000010"), "Belvedere" },
                    { new Guid("20000000-0000-0000-0000-000000000042"), new Guid("10000000-0000-0000-0000-000000000010"), "Sayago" },
                    { new Guid("20000000-0000-0000-0000-000000000043"), new Guid("10000000-0000-0000-0000-000000000010"), "Paso de las Duranas" },
                    { new Guid("20000000-0000-0000-0000-000000000044"), new Guid("10000000-0000-0000-0000-000000000010"), "Aires Puros" },
                    { new Guid("20000000-0000-0000-0000-000000000045"), new Guid("10000000-0000-0000-0000-000000000010"), "Cerrito de la Victoria" },
                    { new Guid("20000000-0000-0000-0000-000000000046"), new Guid("10000000-0000-0000-0000-000000000010"), "Pérez Castellanos" },
                    { new Guid("20000000-0000-0000-0000-000000000047"), new Guid("10000000-0000-0000-0000-000000000010"), "Ituzaingó" },
                    { new Guid("20000000-0000-0000-0000-000000000048"), new Guid("10000000-0000-0000-0000-000000000010"), "La Cruz de Carrasco" },
                    { new Guid("20000000-0000-0000-0000-000000000049"), new Guid("10000000-0000-0000-0000-000000000010"), "Bella Italia" },
                    { new Guid("20000000-0000-0000-0000-000000000050"), new Guid("10000000-0000-0000-0000-000000000010"), "Punta de Rieles" },
                    { new Guid("20000000-0000-0000-0000-000000000051"), new Guid("10000000-0000-0000-0000-000000000010"), "Nueva España" },
                    { new Guid("20000000-0000-0000-0000-000000000052"), new Guid("10000000-0000-0000-0000-000000000010"), "La Chancha" },
                    { new Guid("20000000-0000-0000-0000-000000000053"), new Guid("10000000-0000-0000-0000-000000000010"), "Jardines del Hipódromo" },
                    { new Guid("20000000-0000-0000-0000-000000000054"), new Guid("10000000-0000-0000-0000-000000000010"), "Piedras Blancas" },
                    { new Guid("20000000-0000-0000-0000-000000000055"), new Guid("10000000-0000-0000-0000-000000000010"), "Marconi" },
                    { new Guid("20000000-0000-0000-0000-000000000056"), new Guid("10000000-0000-0000-0000-000000000010"), "Plácido Ellauri" },
                    { new Guid("20000000-0000-0000-0000-000000000057"), new Guid("10000000-0000-0000-0000-000000000010"), "Las Acacias" },
                    { new Guid("20000000-0000-0000-0000-000000000058"), new Guid("10000000-0000-0000-0000-000000000010"), "Casavalle" },
                    { new Guid("20000000-0000-0000-0000-000000000059"), new Guid("10000000-0000-0000-0000-000000000010"), "Manga" },
                    { new Guid("20000000-0000-0000-0000-000000000060"), new Guid("10000000-0000-0000-0000-000000000010"), "Lavalleja" },
                    { new Guid("20000000-0000-0000-0000-000000000061"), new Guid("10000000-0000-0000-0000-000000000010"), "Peñarol" },
                    { new Guid("20000000-0000-0000-0000-000000000062"), new Guid("10000000-0000-0000-0000-000000000010"), "Sayago Norte" },
                    { new Guid("20000000-0000-0000-0000-000000000063"), new Guid("10000000-0000-0000-0000-000000000010"), "Conciliación" },
                    { new Guid("20000000-0000-0000-0000-000000000064"), new Guid("10000000-0000-0000-0000-000000000010"), "Nuevo París" },
                    { new Guid("20000000-0000-0000-0000-000000000065"), new Guid("10000000-0000-0000-0000-000000000010"), "La Teja / Pueblo Victoria" },
                    { new Guid("20000000-0000-0000-0000-000000000066"), new Guid("10000000-0000-0000-0000-000000000010"), "Tres Ombúes" },
                    { new Guid("20000000-0000-0000-0000-000000000067"), new Guid("10000000-0000-0000-0000-000000000010"), "El Tobogán" },
                    { new Guid("20000000-0000-0000-0000-000000000068"), new Guid("10000000-0000-0000-0000-000000000010"), "Cerro Norte" },
                    { new Guid("20000000-0000-0000-0000-000000000069"), new Guid("10000000-0000-0000-0000-000000000010"), "Villa del Cerro" },
                    { new Guid("20000000-0000-0000-0000-000000000070"), new Guid("10000000-0000-0000-0000-000000000010"), "Casabó" },
                    { new Guid("20000000-0000-0000-0000-000000000071"), new Guid("10000000-0000-0000-0000-000000000010"), "Santa Catalina" },
                    { new Guid("20000000-0000-0000-0000-000000000072"), new Guid("10000000-0000-0000-0000-000000000010"), "La Paloma-Tomkinson" },
                    { new Guid("20000000-0000-0000-0000-000000000073"), new Guid("10000000-0000-0000-0000-000000000010"), "Villa Colón" },
                    { new Guid("20000000-0000-0000-0000-000000000074"), new Guid("10000000-0000-0000-0000-000000000010"), "Lezica" },
                    { new Guid("20000000-0000-0000-0000-000000000075"), new Guid("10000000-0000-0000-0000-000000000010"), "Los Bulevares" },
                    { new Guid("20000000-0000-0000-0000-000000000076"), new Guid("10000000-0000-0000-0000-000000000010"), "Paso de la Arena" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$mwvSj2qly83vKXHqNTujF.Urh5UwFgA5OtOJezSy5oFNaqK/dkH3W");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000026"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000027"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000028"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000029"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000030"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000031"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000032"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000033"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000034"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000035"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000036"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000037"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000038"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000039"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000040"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000041"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000042"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000043"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000044"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000045"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000046"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000047"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000048"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000049"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000050"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000051"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000052"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000053"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000054"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000055"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000056"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000057"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000058"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000059"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000060"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000061"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000062"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000063"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000064"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000065"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000066"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000067"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000068"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000069"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000070"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000071"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000072"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000073"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000074"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000075"));

            migrationBuilder.DeleteData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000076"));

            foreach (var id in MontevideoRenameIds)
            {
                migrationBuilder.UpdateData(
                    table: "Barrios",
                    keyColumn: "Id",
                    keyValue: new Guid(id),
                    column: "Nombre",
                    value: $"__tmp_migrating_{id[^4..]}");
            }

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "Nombre",
                value: "Centro");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "Nombre",
                value: "Cordón");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                column: "Nombre",
                value: "Ciudad Vieja");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                column: "Nombre",
                value: "Palermo");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                column: "Nombre",
                value: "Parque Rodó");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000006"),
                column: "Nombre",
                value: "Pocitos");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000008"),
                column: "Nombre",
                value: "Malvín");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000010"),
                column: "Nombre",
                value: "Punta Gorda");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000011"),
                column: "Nombre",
                value: "Carrasco");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000012"),
                column: "Nombre",
                value: "Cerro");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000013"),
                column: "Nombre",
                value: "Aguada");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000014"),
                column: "Nombre",
                value: "Reducto");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000015"),
                column: "Nombre",
                value: "La Blanqueada");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000016"),
                column: "Nombre",
                value: "Prado");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000017"),
                column: "Nombre",
                value: "Tajamar");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000018"),
                column: "Nombre",
                value: "La Comercial");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000019"),
                column: "Nombre",
                value: "Brazo Oriental");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000020"),
                column: "Nombre",
                value: "Belvedere");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000021"),
                column: "Nombre",
                value: "Capurro");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000022"),
                column: "Nombre",
                value: "Jacinto Vera");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000023"),
                column: "Nombre",
                value: "Paso de las Duranas");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000024"),
                column: "Nombre",
                value: "Piedras Blancas");

            migrationBuilder.UpdateData(
                table: "Barrios",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000025"),
                column: "Nombre",
                value: "Casavalle");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$DTPwUNcGef0JPT76P78iUeEOZYUCXvvwga/ssMCpi4CqiUS8eNKTG");
        }
    }
}
