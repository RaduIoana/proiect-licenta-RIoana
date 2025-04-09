using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proiect_licenta.Migrations
{
    /// <inheritdoc />
    public partial class relationmig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Installs_MyUsers_MyUserId",
                table: "Installs");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRecords_MyUsers_MyUserId",
                table: "PaymentRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_RefundRequests_MyUsers_MyUserId",
                table: "RefundRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_MyUsers_MyUserId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_MyUsers_MyUserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_MyUserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reports_MyUserId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_RefundRequests_MyUserId",
                table: "RefundRequests");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRecords_MyUserId",
                table: "PaymentRecords");

            migrationBuilder.DropIndex(
                name: "IX_Installs_MyUserId",
                table: "Installs");

            migrationBuilder.DropColumn(
                name: "MyUserId",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "MyUserId",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "MyUserId",
                table: "RefundRequests");

            migrationBuilder.DropColumn(
                name: "MyUserId",
                table: "PaymentRecords");

            migrationBuilder.DropColumn(
                name: "MyUserId",
                table: "Installs");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Reviews",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Reports",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "RefundRequests",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "PaymentRecords",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_AppId",
                table: "Reports",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_PaymentId",
                table: "RefundRequests",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_UserId",
                table: "RefundRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_AppId",
                table: "PaymentRecords",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_UserId",
                table: "PaymentRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Installs_AppId",
                table: "Installs",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_Installs_PaymentId",
                table: "Installs",
                column: "PaymentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Installs_Apps_AppId",
                table: "Installs",
                column: "AppId",
                principalTable: "Apps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Installs_MyUsers_UserId",
                table: "Installs",
                column: "UserId",
                principalTable: "MyUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Installs_PaymentRecords_PaymentId",
                table: "Installs",
                column: "PaymentId",
                principalTable: "PaymentRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRecords_Apps_AppId",
                table: "PaymentRecords",
                column: "AppId",
                principalTable: "Apps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRecords_MyUsers_UserId",
                table: "PaymentRecords",
                column: "UserId",
                principalTable: "MyUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefundRequests_MyUsers_UserId",
                table: "RefundRequests",
                column: "UserId",
                principalTable: "MyUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefundRequests_PaymentRecords_PaymentId",
                table: "RefundRequests",
                column: "PaymentId",
                principalTable: "PaymentRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_Apps_AppId",
                table: "Reports",
                column: "AppId",
                principalTable: "Apps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_MyUsers_UserId",
                table: "Reports",
                column: "UserId",
                principalTable: "MyUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Apps_AppId",
                table: "Reviews",
                column: "AppId",
                principalTable: "Apps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_MyUsers_UserId",
                table: "Reviews",
                column: "UserId",
                principalTable: "MyUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Installs_Apps_AppId",
                table: "Installs");

            migrationBuilder.DropForeignKey(
                name: "FK_Installs_MyUsers_UserId",
                table: "Installs");

            migrationBuilder.DropForeignKey(
                name: "FK_Installs_PaymentRecords_PaymentId",
                table: "Installs");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRecords_Apps_AppId",
                table: "PaymentRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRecords_MyUsers_UserId",
                table: "PaymentRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_RefundRequests_MyUsers_UserId",
                table: "RefundRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RefundRequests_PaymentRecords_PaymentId",
                table: "RefundRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_Apps_AppId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reports_MyUsers_UserId",
                table: "Reports");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Apps_AppId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_MyUsers_UserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_UserId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reports_AppId",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_RefundRequests_PaymentId",
                table: "RefundRequests");

            migrationBuilder.DropIndex(
                name: "IX_RefundRequests_UserId",
                table: "RefundRequests");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRecords_AppId",
                table: "PaymentRecords");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRecords_UserId",
                table: "PaymentRecords");

            migrationBuilder.DropIndex(
                name: "IX_Installs_AppId",
                table: "Installs");

            migrationBuilder.DropIndex(
                name: "IX_Installs_PaymentId",
                table: "Installs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RefundRequests");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Reviews",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MyUserId",
                table: "Reviews",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Reports",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MyUserId",
                table: "Reports",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MyUserId",
                table: "RefundRequests",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "PaymentRecords",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MyUserId",
                table: "PaymentRecords",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MyUserId",
                table: "Installs",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_MyUserId",
                table: "Reviews",
                column: "MyUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_MyUserId",
                table: "Reports",
                column: "MyUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundRequests_MyUserId",
                table: "RefundRequests",
                column: "MyUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRecords_MyUserId",
                table: "PaymentRecords",
                column: "MyUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Installs_MyUserId",
                table: "Installs",
                column: "MyUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Installs_MyUsers_MyUserId",
                table: "Installs",
                column: "MyUserId",
                principalTable: "MyUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRecords_MyUsers_MyUserId",
                table: "PaymentRecords",
                column: "MyUserId",
                principalTable: "MyUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RefundRequests_MyUsers_MyUserId",
                table: "RefundRequests",
                column: "MyUserId",
                principalTable: "MyUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_MyUsers_MyUserId",
                table: "Reports",
                column: "MyUserId",
                principalTable: "MyUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_MyUsers_MyUserId",
                table: "Reviews",
                column: "MyUserId",
                principalTable: "MyUsers",
                principalColumn: "Id");
        }
    }
}
