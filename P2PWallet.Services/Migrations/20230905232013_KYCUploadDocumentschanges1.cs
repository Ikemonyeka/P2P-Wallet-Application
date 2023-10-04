using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class KYCUploadDocumentschanges1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KYCUpload_KYCRequiredDocuments_KycRecId",
                table: "KYCUpload");

            migrationBuilder.DropForeignKey(
                name: "FK_KYCUpload_Users_userId",
                table: "KYCUpload");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KYCUpload",
                table: "KYCUpload");

            migrationBuilder.RenameTable(
                name: "KYCUpload",
                newName: "kYCUploads");

            migrationBuilder.RenameIndex(
                name: "IX_KYCUpload_userId",
                table: "kYCUploads",
                newName: "IX_kYCUploads_userId");

            migrationBuilder.RenameIndex(
                name: "IX_KYCUpload_KycRecId",
                table: "kYCUploads",
                newName: "IX_kYCUploads_KycRecId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_kYCUploads",
                table: "kYCUploads",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_kYCUploads_KYCRequiredDocuments_KycRecId",
                table: "kYCUploads",
                column: "KycRecId",
                principalTable: "KYCRequiredDocuments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_kYCUploads_Users_userId",
                table: "kYCUploads",
                column: "userId",
                principalTable: "Users",
                principalColumn: "userId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_kYCUploads_KYCRequiredDocuments_KycRecId",
                table: "kYCUploads");

            migrationBuilder.DropForeignKey(
                name: "FK_kYCUploads_Users_userId",
                table: "kYCUploads");

            migrationBuilder.DropPrimaryKey(
                name: "PK_kYCUploads",
                table: "kYCUploads");

            migrationBuilder.RenameTable(
                name: "kYCUploads",
                newName: "KYCUpload");

            migrationBuilder.RenameIndex(
                name: "IX_kYCUploads_userId",
                table: "KYCUpload",
                newName: "IX_KYCUpload_userId");

            migrationBuilder.RenameIndex(
                name: "IX_kYCUploads_KycRecId",
                table: "KYCUpload",
                newName: "IX_KYCUpload_KycRecId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KYCUpload",
                table: "KYCUpload",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_KYCUpload_KYCRequiredDocuments_KycRecId",
                table: "KYCUpload",
                column: "KycRecId",
                principalTable: "KYCRequiredDocuments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_KYCUpload_Users_userId",
                table: "KYCUpload",
                column: "userId",
                principalTable: "Users",
                principalColumn: "userId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
