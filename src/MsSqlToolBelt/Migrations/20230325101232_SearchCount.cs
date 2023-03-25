using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MsSqlToolBelt.Migrations
{
    /// <inheritdoc />
    public partial class SearchCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SearchCount",
                table: "SearchHistory",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SearchCount",
                table: "SearchHistory");
        }
    }
}
