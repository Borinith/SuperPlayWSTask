using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuperPlayServer.Migrations
{
    /// <inheritdoc />
    public partial class IndexForPlayerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Devices_PlayerId",
                table: "Devices",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Devices_PlayerId",
                table: "Devices");
        }
    }
}
