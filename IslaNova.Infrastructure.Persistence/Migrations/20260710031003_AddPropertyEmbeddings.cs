using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IslaNova.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyEmbeddings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: The pgvector extension is enabled via the Supabase dashboard (requires supabase_admin).
            // EF Core cannot CREATE EXTENSION in Supabase — the postgres user lacks superuser privileges.
            // Extension must be enabled manually before running this migration.

            migrationBuilder.CreateTable(
                name: "PropertyEmbeddings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropertyId = table.Column<int>(type: "integer", nullable: false),
                    PlainText = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyEmbeddings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyEmbeddings_PropertyId",
                table: "PropertyEmbeddings",
                column: "PropertyId",
                unique: true);

            // The embedding column (vector(1536)) is managed via raw SQL — not tracked by EF Core.
            // Requires pgvector extension to be enabled in the database (handled above by AlterDatabase).
            migrationBuilder.Sql(
                """ALTER TABLE "PropertyEmbeddings" ADD COLUMN embedding vector(1536) NULL;""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyEmbeddings");
            // NOTE: We intentionally do NOT drop the vector extension here,
            // as it was enabled externally via the Supabase dashboard.
        }
    }
}
