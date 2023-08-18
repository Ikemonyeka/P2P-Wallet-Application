using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace P2PWallet.Services.Migrations
{
    /// <inheritdoc />
    public partial class LockedandUnclockSeedHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LockedUnlockedDescriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userId = table.Column<int>(type: "int", nullable: false),
                    DescriptionId = table.Column<int>(type: "int", nullable: false),
                    LockedUnlockedUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LockedUnlockedDescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LockedUnlockedDescriptions_Descriptions_LockedUnlockedUserId",
                        column: x => x.LockedUnlockedUserId,
                        principalTable: "Descriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LockedUnlockedDescriptions_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "userId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LockedUnlockedDescriptions_LockedUnlockedUserId",
                table: "LockedUnlockedDescriptions",
                column: "LockedUnlockedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LockedUnlockedDescriptions_userId",
                table: "LockedUnlockedDescriptions",
                column: "userId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LockedUnlockedDescriptions");
        }
    }
}
