using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class edit2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LockedUnlockedAccountsDescriptions_Descriptions_LockedUnlockedUserId",
                table: "LockedUnlockedAccountsDescriptions");

            migrationBuilder.DropIndex(
                name: "IX_LockedUnlockedAccountsDescriptions_LockedUnlockedUserId",
                table: "LockedUnlockedAccountsDescriptions");

            migrationBuilder.DropColumn(
                name: "LockedUnlockedUserId",
                table: "LockedUnlockedAccountsDescriptions");

            migrationBuilder.CreateIndex(
                name: "IX_LockedUnlockedAccountsDescriptions_DescriptionId",
                table: "LockedUnlockedAccountsDescriptions",
                column: "DescriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_LockedUnlockedAccountsDescriptions_Descriptions_DescriptionId",
                table: "LockedUnlockedAccountsDescriptions",
                column: "DescriptionId",
                principalTable: "Descriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LockedUnlockedAccountsDescriptions_Descriptions_DescriptionId",
                table: "LockedUnlockedAccountsDescriptions");

            migrationBuilder.DropIndex(
                name: "IX_LockedUnlockedAccountsDescriptions_DescriptionId",
                table: "LockedUnlockedAccountsDescriptions");

            migrationBuilder.AddColumn<int>(
                name: "LockedUnlockedUserId",
                table: "LockedUnlockedAccountsDescriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LockedUnlockedAccountsDescriptions_LockedUnlockedUserId",
                table: "LockedUnlockedAccountsDescriptions",
                column: "LockedUnlockedUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LockedUnlockedAccountsDescriptions_Descriptions_LockedUnlockedUserId",
                table: "LockedUnlockedAccountsDescriptions",
                column: "LockedUnlockedUserId",
                principalTable: "Descriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
