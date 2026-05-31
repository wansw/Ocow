using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocow.Files.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialFileUpload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "file_resources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ObjectKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BucketName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Region = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FileCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MimeType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Extension = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    StorageType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Local"),
                    BizType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BizId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UploaderId = table.Column<Guid>(type: "uuid", nullable: true),
                    UploaderScope = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Normal"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_file_resources", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_file_resources_BizType_BizId",
                table: "file_resources",
                columns: new[] { "BizType", "BizId" });

            migrationBuilder.CreateIndex(
                name: "IX_file_resources_CreatedAt",
                table: "file_resources",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_file_resources_ObjectKey",
                table: "file_resources",
                column: "ObjectKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "file_resources");
        }
    }
}
