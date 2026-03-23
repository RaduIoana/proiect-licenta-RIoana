using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proiect_licenta.Migrations
{
    /// <inheritdoc />
    public partial class devIdmig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DevId",
                table: "Apps",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DevId",
                table: "Apps");
        }
    }
}
