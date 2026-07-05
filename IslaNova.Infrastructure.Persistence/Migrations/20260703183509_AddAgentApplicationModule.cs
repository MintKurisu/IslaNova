using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IslaNova.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentApplicationModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgentApplications",
                columns: table => new
                {
                    ApplicationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    LicenseNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CertificationNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EmploymentType = table.Column<int>(type: "integer", nullable: false),
                    AgencyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProfessionalStatement = table.Column<string>(type: "character varying(1500)", maxLength: 1500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    AdminComments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentApplications", x => x.ApplicationId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentApplications_Status",
                table: "AgentApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AgentApplications_UserId",
                table: "AgentApplications",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentApplications");
        }
    }
}
