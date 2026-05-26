using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookIt.API.Migrations
{
    /// <inheritdoc />
    public partial class AddReservasDireccionesYGeografia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DireccionId",
                table: "Services",
                type: "uuid",
                nullable: true);

            

            migrationBuilder.CreateTable(
                name: "Departamentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departamentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Confirmada = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FechaReservaCliente = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservas_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Barrios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartamentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Barrios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Barrios_Departamentos_DepartamentoId",
                        column: x => x.DepartamentoId,
                        principalTable: "Departamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Direcciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepartamentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    BarrioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Calle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Direcciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Direcciones_Barrios_BarrioId",
                        column: x => x.BarrioId,
                        principalTable: "Barrios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Direcciones_Departamentos_DepartamentoId",
                        column: x => x.DepartamentoId,
                        principalTable: "Departamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Departamentos",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "Artigas" },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "Canelones" },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "Cerro Largo" },
                    { new Guid("10000000-0000-0000-0000-000000000004"), "Colonia" },
                    { new Guid("10000000-0000-0000-0000-000000000005"), "Durazno" },
                    { new Guid("10000000-0000-0000-0000-000000000006"), "Flores" },
                    { new Guid("10000000-0000-0000-0000-000000000007"), "Florida" },
                    { new Guid("10000000-0000-0000-0000-000000000008"), "Lavalleja" },
                    { new Guid("10000000-0000-0000-0000-000000000009"), "Maldonado" },
                    { new Guid("10000000-0000-0000-0000-000000000010"), "Montevideo" },
                    { new Guid("10000000-0000-0000-0000-000000000011"), "Paysandú" },
                    { new Guid("10000000-0000-0000-0000-000000000012"), "Río Negro" },
                    { new Guid("10000000-0000-0000-0000-000000000013"), "Rivera" },
                    { new Guid("10000000-0000-0000-0000-000000000014"), "Rocha" },
                    { new Guid("10000000-0000-0000-0000-000000000015"), "Salto" },
                    { new Guid("10000000-0000-0000-0000-000000000016"), "San José" },
                    { new Guid("10000000-0000-0000-0000-000000000017"), "Soriano" },
                    { new Guid("10000000-0000-0000-0000-000000000018"), "Tacuarembó" },
                    { new Guid("10000000-0000-0000-0000-000000000019"), "Treinta y Tres" }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$Sew5qqQei5YGbBlMLeoty.Jpf69NJxNouPrMT1PqhiWaIRShs3O2.");

            migrationBuilder.InsertData(
                table: "Barrios",
                columns: new[] { "Id", "DepartamentoId", "Nombre" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), new Guid("10000000-0000-0000-0000-000000000010"), "Centro" },
                    { new Guid("20000000-0000-0000-0000-000000000002"), new Guid("10000000-0000-0000-0000-000000000010"), "Cordón" },
                    { new Guid("20000000-0000-0000-0000-000000000003"), new Guid("10000000-0000-0000-0000-000000000010"), "Ciudad Vieja" },
                    { new Guid("20000000-0000-0000-0000-000000000004"), new Guid("10000000-0000-0000-0000-000000000010"), "Palermo" },
                    { new Guid("20000000-0000-0000-0000-000000000005"), new Guid("10000000-0000-0000-0000-000000000010"), "Parque Rodó" },
                    { new Guid("20000000-0000-0000-0000-000000000006"), new Guid("10000000-0000-0000-0000-000000000010"), "Pocitos" },
                    { new Guid("20000000-0000-0000-0000-000000000007"), new Guid("10000000-0000-0000-0000-000000000010"), "Punta Carretas" },
                    { new Guid("20000000-0000-0000-0000-000000000008"), new Guid("10000000-0000-0000-0000-000000000010"), "Malvín" },
                    { new Guid("20000000-0000-0000-0000-000000000009"), new Guid("10000000-0000-0000-0000-000000000010"), "Buceo" },
                    { new Guid("20000000-0000-0000-0000-000000000010"), new Guid("10000000-0000-0000-0000-000000000010"), "Punta Gorda" },
                    { new Guid("20000000-0000-0000-0000-000000000011"), new Guid("10000000-0000-0000-0000-000000000010"), "Carrasco" },
                    { new Guid("20000000-0000-0000-0000-000000000012"), new Guid("10000000-0000-0000-0000-000000000010"), "Cerro" },
                    { new Guid("20000000-0000-0000-0000-000000000013"), new Guid("10000000-0000-0000-0000-000000000010"), "Aguada" },
                    { new Guid("20000000-0000-0000-0000-000000000014"), new Guid("10000000-0000-0000-0000-000000000010"), "Reducto" },
                    { new Guid("20000000-0000-0000-0000-000000000015"), new Guid("10000000-0000-0000-0000-000000000010"), "La Blanqueada" },
                    { new Guid("20000000-0000-0000-0000-000000000016"), new Guid("10000000-0000-0000-0000-000000000010"), "Prado" },
                    { new Guid("20000000-0000-0000-0000-000000000017"), new Guid("10000000-0000-0000-0000-000000000010"), "Tajamar" },
                    { new Guid("20000000-0000-0000-0000-000000000018"), new Guid("10000000-0000-0000-0000-000000000010"), "La Comercial" },
                    { new Guid("20000000-0000-0000-0000-000000000019"), new Guid("10000000-0000-0000-0000-000000000010"), "Brazo Oriental" },
                    { new Guid("20000000-0000-0000-0000-000000000020"), new Guid("10000000-0000-0000-0000-000000000010"), "Belvedere" },
                    { new Guid("20000000-0000-0000-0000-000000000021"), new Guid("10000000-0000-0000-0000-000000000010"), "Capurro" },
                    { new Guid("20000000-0000-0000-0000-000000000022"), new Guid("10000000-0000-0000-0000-000000000010"), "Jacinto Vera" },
                    { new Guid("20000000-0000-0000-0000-000000000023"), new Guid("10000000-0000-0000-0000-000000000010"), "Paso de las Duranas" },
                    { new Guid("20000000-0000-0000-0000-000000000024"), new Guid("10000000-0000-0000-0000-000000000010"), "Piedras Blancas" },
                    { new Guid("20000000-0000-0000-0000-000000000025"), new Guid("10000000-0000-0000-0000-000000000010"), "Casavalle" },
                    { new Guid("20000000-0000-0000-0000-000000000101"), new Guid("10000000-0000-0000-0000-000000000002"), "Ciudad de la Costa" },
                    { new Guid("20000000-0000-0000-0000-000000000102"), new Guid("10000000-0000-0000-0000-000000000002"), "Las Piedras" },
                    { new Guid("20000000-0000-0000-0000-000000000103"), new Guid("10000000-0000-0000-0000-000000000002"), "Pando" },
                    { new Guid("20000000-0000-0000-0000-000000000104"), new Guid("10000000-0000-0000-0000-000000000002"), "Barros Blancos" },
                    { new Guid("20000000-0000-0000-0000-000000000105"), new Guid("10000000-0000-0000-0000-000000000002"), "La Paz" },
                    { new Guid("20000000-0000-0000-0000-000000000106"), new Guid("10000000-0000-0000-0000-000000000002"), "Santa Lucía" },
                    { new Guid("20000000-0000-0000-0000-000000000201"), new Guid("10000000-0000-0000-0000-000000000009"), "Maldonado" },
                    { new Guid("20000000-0000-0000-0000-000000000202"), new Guid("10000000-0000-0000-0000-000000000009"), "Punta del Este" },
                    { new Guid("20000000-0000-0000-0000-000000000203"), new Guid("10000000-0000-0000-0000-000000000009"), "San Carlos" },
                    { new Guid("20000000-0000-0000-0000-000000000204"), new Guid("10000000-0000-0000-0000-000000000009"), "Piriápolis" },
                    { new Guid("20000000-0000-0000-0000-000000000205"), new Guid("10000000-0000-0000-0000-000000000009"), "La Barra" },
                    { new Guid("20000000-0000-0000-0000-000000000301"), new Guid("10000000-0000-0000-0000-000000000001"), "Artigas" },
                    { new Guid("20000000-0000-0000-0000-000000000302"), new Guid("10000000-0000-0000-0000-000000000003"), "Cerro Largo" },
                    { new Guid("20000000-0000-0000-0000-000000000303"), new Guid("10000000-0000-0000-0000-000000000004"), "Colonia" },
                    { new Guid("20000000-0000-0000-0000-000000000304"), new Guid("10000000-0000-0000-0000-000000000005"), "Durazno" },
                    { new Guid("20000000-0000-0000-0000-000000000305"), new Guid("10000000-0000-0000-0000-000000000006"), "Flores" },
                    { new Guid("20000000-0000-0000-0000-000000000306"), new Guid("10000000-0000-0000-0000-000000000007"), "Florida" },
                    { new Guid("20000000-0000-0000-0000-000000000307"), new Guid("10000000-0000-0000-0000-000000000008"), "Lavalleja" },
                    { new Guid("20000000-0000-0000-0000-000000000308"), new Guid("10000000-0000-0000-0000-000000000011"), "Paysandú" },
                    { new Guid("20000000-0000-0000-0000-000000000309"), new Guid("10000000-0000-0000-0000-000000000012"), "Río Negro" },
                    { new Guid("20000000-0000-0000-0000-000000000310"), new Guid("10000000-0000-0000-0000-000000000013"), "Rivera" },
                    { new Guid("20000000-0000-0000-0000-000000000311"), new Guid("10000000-0000-0000-0000-000000000014"), "Rocha" },
                    { new Guid("20000000-0000-0000-0000-000000000312"), new Guid("10000000-0000-0000-0000-000000000015"), "Salto" },
                    { new Guid("20000000-0000-0000-0000-000000000313"), new Guid("10000000-0000-0000-0000-000000000016"), "San José" },
                    { new Guid("20000000-0000-0000-0000-000000000314"), new Guid("10000000-0000-0000-0000-000000000017"), "Soriano" },
                    { new Guid("20000000-0000-0000-0000-000000000315"), new Guid("10000000-0000-0000-0000-000000000018"), "Tacuarembó" },
                    { new Guid("20000000-0000-0000-0000-000000000316"), new Guid("10000000-0000-0000-0000-000000000019"), "Treinta y Tres" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Services_DireccionId",
                table: "Services",
                column: "DireccionId");

            migrationBuilder.CreateIndex(
                name: "IX_Barrios_DepartamentoId_Nombre",
                table: "Barrios",
                columns: new[] { "DepartamentoId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departamentos_Nombre",
                table: "Departamentos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Direcciones_BarrioId",
                table: "Direcciones",
                column: "BarrioId");

            migrationBuilder.CreateIndex(
                name: "IX_Direcciones_DepartamentoId",
                table: "Direcciones",
                column: "DepartamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_ServiceId",
                table: "Reservas",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_UserId",
                table: "Reservas",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Direcciones_DireccionId",
                table: "Services",
                column: "DireccionId",
                principalTable: "Direcciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Direcciones_DireccionId",
                table: "Services");

            migrationBuilder.DropTable(
                name: "Direcciones");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Barrios");

            migrationBuilder.DropTable(
                name: "Departamentos");

            migrationBuilder.DropIndex(
                name: "IX_Services_DireccionId",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "DireccionId",
                table: "Services");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$k4btkcarpll1ZVUBBxgebOqvuKnG9qGqqDpH6N2r39.h5Tuvrulrm");
        }
    }
}
