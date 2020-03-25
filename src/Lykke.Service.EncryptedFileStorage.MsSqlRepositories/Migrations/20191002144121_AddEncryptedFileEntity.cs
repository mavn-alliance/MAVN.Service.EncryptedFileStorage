using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Lykke.Service.EncryptedFileStorage.MsSqlRepositories.Migrations
{
    public partial class AddEncryptedFileEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "encrypted_files_storage");

            migrationBuilder.CreateTable(
                name: "encrypted_files",
                schema: "encrypted_files_storage",
                columns: table => new
                {
                    FileId = table.Column<Guid>(nullable: false, defaultValueSql: "newid()"),
                    Origin = table.Column<string>(maxLength: 200, nullable: false),
                    FileName = table.Column<string>(maxLength: 255, nullable: false),
                    BlobName = table.Column<string>(maxLength: 255, nullable: false),
                    Length = table.Column<long>(nullable: false),
                    FileDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_encrypted_files", x => x.FileId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_encrypted_files_Origin_FileName",
                schema: "encrypted_files_storage",
                table: "encrypted_files",
                columns: new[] { "Origin", "FileName" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "encrypted_files",
                schema: "encrypted_files_storage");
        }
    }
}
