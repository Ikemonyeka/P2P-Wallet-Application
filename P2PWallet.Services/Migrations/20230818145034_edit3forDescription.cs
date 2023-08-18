using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class edit3forDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LockedUnlockedAccountsDescriptions_Descriptions_DescriptionId",
                table: "LockedUnlockedAccountsDescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_LockedUnlockedAccountsDescriptions_Users_userId",
                table: "LockedUnlockedAccountsDescriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LockedUnlockedAccountsDescriptions",
                table: "LockedUnlockedAccountsDescriptions");

            migrationBuilder.RenameTable(
                name: "LockedUnlockedAccountsDescriptions",
                newName: "LockedUnlockedDescriptions");

            migrationBuilder.RenameIndex(
                name: "IX_LockedUnlockedAccountsDescriptions_userId",
                table: "LockedUnlockedDescriptions",
                newName: "IX_LockedUnlockedDescriptions_userId");

            migrationBuilder.RenameIndex(
                name: "IX_LockedUnlockedAccountsDescriptions_DescriptionId",
                table: "LockedUnlockedDescriptions",
                newName: "IX_LockedUnlockedDescriptions_DescriptionId");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "LockedUnlockedDescriptions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_LockedUnlockedDescriptions",
                table: "LockedUnlockedDescriptions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LockedUnlockedDescriptions_Descriptions_DescriptionId",
                table: "LockedUnlockedDescriptions",
                column: "DescriptionId",
                principalTable: "Descriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LockedUnlockedDescriptions_Users_userId",
                table: "LockedUnlockedDescriptions",
                column: "userId",
                principalTable: "Users",
                principalColumn: "userId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LockedUnlockedDescriptions_Descriptions_DescriptionId",
                table: "LockedUnlockedDescriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_LockedUnlockedDescriptions_Users_userId",
                table: "LockedUnlockedDescriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LockedUnlockedDescriptions",
                table: "LockedUnlockedDescriptions");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "LockedUnlockedDescriptions");

            migrationBuilder.RenameTable(
                name: "LockedUnlockedDescriptions",
                newName: "LockedUnlockedAccountsDescriptions");

            migrationBuilder.RenameIndex(
                name: "IX_LockedUnlockedDescriptions_userId",
                table: "LockedUnlockedAccountsDescriptions",
                newName: "IX_LockedUnlockedAccountsDescriptions_userId");

            migrationBuilder.RenameIndex(
                name: "IX_LockedUnlockedDescriptions_DescriptionId",
                table: "LockedUnlockedAccountsDescriptions",
                newName: "IX_LockedUnlockedAccountsDescriptions_DescriptionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LockedUnlockedAccountsDescriptions",
                table: "LockedUnlockedAccountsDescriptions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LockedUnlockedAccountsDescriptions_Descriptions_DescriptionId",
                table: "LockedUnlockedAccountsDescriptions",
                column: "DescriptionId",
                principalTable: "Descriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LockedUnlockedAccountsDescriptions_Users_userId",
                table: "LockedUnlockedAccountsDescriptions",
                column: "userId",
                principalTable: "Users",
                principalColumn: "userId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
