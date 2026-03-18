using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreBackendApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameExipresAtToExpiresAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExipresAt",
                table: "RefreshTokens",
                newName: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExpiresAt",
                table: "RefreshTokens",
                newName: "ExipresAt");
        }
    }
}
