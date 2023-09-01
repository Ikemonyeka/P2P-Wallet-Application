using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class chatUpdate5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Admin_adminId",
                table: "Chat");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Admin_adminId",
                table: "Chat",
                column: "adminId",
                principalTable: "Admin",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Admin_adminId",
                table: "Chat");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Admin_adminId",
                table: "Chat",
                column: "adminId",
                principalTable: "Admin",
                principalColumn: "Id");
        }
    }
}
