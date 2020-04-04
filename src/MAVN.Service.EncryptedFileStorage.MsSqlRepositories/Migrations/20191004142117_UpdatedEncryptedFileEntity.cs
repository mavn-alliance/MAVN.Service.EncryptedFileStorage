using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MAVN.Service.EncryptedFileStorage.MsSqlRepositories.Migrations
{
    public partial class UpdatedEncryptedFileEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Length",
                schema: "encrypted_files_storage",
                table: "encrypted_files",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<DateTime>(
                name: "FileDate",
                schema: "encrypted_files_storage",
                table: "encrypted_files",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<string>(
                name: "BlobName",
                schema: "encrypted_files_storage",
                table: "encrypted_files",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 255);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Length",
                schema: "encrypted_files_storage",
                table: "encrypted_files",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "FileDate",
                schema: "encrypted_files_storage",
                table: "encrypted_files",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BlobName",
                schema: "encrypted_files_storage",
                table: "encrypted_files",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
