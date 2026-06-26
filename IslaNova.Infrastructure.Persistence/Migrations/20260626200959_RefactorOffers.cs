using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IslaNova.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Offers");

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Offers",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "Offers",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Offers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Offers");

            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "Offers",
                type: "character varying(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");
        }
    }
}
