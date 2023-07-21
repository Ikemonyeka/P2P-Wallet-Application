using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class FKeyChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_securityQuestions_userId",
                table: "securityQuestions");

            migrationBuilder.CreateIndex(
                name: "IX_securityQuestions_userId",
                table: "securityQuestions",
                column: "userId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_securityQuestions_userId",
                table: "securityQuestions");

            migrationBuilder.CreateIndex(
                name: "IX_securityQuestions_userId",
                table: "securityQuestions",
                column: "userId");
        }
    }
}
