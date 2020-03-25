using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.EncryptedFileStorage.MsSqlRepositories.Migrations
{
    public partial class ReducedOriginSize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Origin",
                schema: "encrypted_files_storage",
                table: "encrypted_files",
                maxLength: 63,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 200);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Origin",
                schema: "encrypted_files_storage",
                table: "encrypted_files",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 63);
        }
    }
}
