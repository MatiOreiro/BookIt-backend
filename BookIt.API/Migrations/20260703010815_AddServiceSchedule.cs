using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookIt.API.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiasAtencionJson",
                table: "Services",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HoraAperturaReserva",
                table: "Services",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HoraAperturaVisita",
                table: "Services",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HoraCierreReserva",
                table: "Services",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HoraCierreVisita",
                table: "Services",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$aSLTqz5X/6sDh2YUdw/Ii.usJZz6tWZ40D1JDq..MWaJ2ZshFyDCu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiasAtencionJson",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "HoraAperturaReserva",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "HoraAperturaVisita",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "HoraCierreReserva",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "HoraCierreVisita",
                table: "Services");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$crYXd24eYqvga4f2vAF2SuiXj.Y2wMX3el.6KUDbOQpR2sBXg64pC");
        }
    }
}
