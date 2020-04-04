using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.EncryptedFileStorage.MsSqlRepositories.Migrations
{
    public partial class AddedIsUploadCompletedToEncryptedFileEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsUploadCompleted",
                schema: "encrypted_files_storage",
                table: "encrypted_files",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsUploadCompleted",
                schema: "encrypted_files_storage",
                table: "encrypted_files");
        }
    }
}
