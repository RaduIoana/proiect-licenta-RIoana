using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proiect_licenta.Migrations
{
    /// <inheritdoc />
    public partial class refundAlter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tx",
                table: "RefundRequests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tx",
                table: "RefundRequests");
        }
    }
}
