using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proiect_licenta.Migrations
{
    /// <inheritdoc />
    public partial class paymentlicensealtermig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRecords_Licenses_LicenseId",
                table: "PaymentRecords");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRecords_LicenseId",
                table: "PaymentRecords");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_LicenseId",
                table: "PaymentRecords",
                column: "LicenseId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRecords_Licenses_LicenseId",
                table: "PaymentRecords",
                column: "LicenseId",
                principalTable: "Licenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRecords_Licenses_LicenseId",
                table: "PaymentRecords");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRecords_LicenseId",
                table: "PaymentRecords");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_LicenseId",
                table: "PaymentRecords",
                column: "LicenseId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRecords_Licenses_LicenseId",
                table: "PaymentRecords",
                column: "LicenseId",
                principalTable: "Licenses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
