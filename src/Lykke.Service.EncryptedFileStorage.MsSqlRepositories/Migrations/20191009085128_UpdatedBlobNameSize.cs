using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.EncryptedFileStorage.MsSqlRepositories.Migrations
{
    public partial class UpdatedBlobNameSize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BlobName",
                schema: "encrypted_files_storage",
                table: "encrypted_files",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 255,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BlobName",
                schema: "encrypted_files_storage",
                table: "encrypted_files",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 1024,
                oldNullable: true);
        }
    }
}
