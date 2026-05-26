using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using BookIt.API.Data;

#nullable disable

namespace BookIt.API.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260525120000_AddProfileAndServiceImages")]
public partial class AddProfileAndServiceImages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ProfileImageUrl",
            table: "Users",
            type: "character varying(400)",
            maxLength: 400,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "ImageUrlsJson",
            table: "Services",
            type: "text",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ProfileImageUrl",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "ImageUrlsJson",
            table: "Services");
    }
}
