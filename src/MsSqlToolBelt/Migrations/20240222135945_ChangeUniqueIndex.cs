using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable IDE0300 // Simplify collection initialization
#pragma warning disable CA1861 // Avoid constant arrays as arguments

namespace MsSqlToolBelt.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Server_Name",
                table: "Server");


            migrationBuilder.CreateIndex(
                name: "IX_Server_Name_DefaultDatabase",
                table: "Server",
                columns: new[] { "Name", "DefaultDatabase" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Server_Name_DefaultDatabase",
                table: "Server");

            migrationBuilder.CreateIndex(
                name: "IX_Server_Name",
                table: "Server",
                column: "Name",
                unique: true);
        }
    }
}
#pragma warning restore CA1861 // Avoid constant arrays as arguments
#pragma warning restore IDE0300 // Simplify collection initialization