using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proiect_licenta.Migrations
{
    /// <inheritdoc />
    public partial class refundsmig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnSum",
                table: "RefundRequests");

            migrationBuilder.AddColumn<double>(
                name: "PaymentAmount",
                table: "PaymentRecords",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentAmount",
                table: "PaymentRecords");

            migrationBuilder.AddColumn<int>(
                name: "ReturnSum",
                table: "RefundRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
