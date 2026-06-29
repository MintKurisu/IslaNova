using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IslaNova.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgentProfiles",
                columns: table => new
                {
                    AgentProfileId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AgentId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Bio = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    YearsOfExperience = table.Column<int>(type: "integer", nullable: true),
                    WhatsappNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    FacebookUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    InstagramUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    SpecialtyZones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentProfiles", x => x.AgentProfileId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentProfiles_AgentId",
                table: "AgentProfiles",
                column: "AgentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentProfiles");
        }
    }
}
