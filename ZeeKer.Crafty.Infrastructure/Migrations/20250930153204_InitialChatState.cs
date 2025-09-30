using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ZeeKer.Crafty.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialChatState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TelegramChatStates",
                columns: table => new
                {
                    ChatId = table.Column<long>(type: "INTEGER", nullable: false),
                    LastMessageId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramChatStates", x => x.ChatId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TelegramChatStates_ChatId",
                table: "TelegramChatStates",
                column: "ChatId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TelegramChatStates");
        }
    }
}
