using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ocow.Jobs.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "job_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    JobType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Cron = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetService = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    TargetApi = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    HttpMethod = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    RequestBody = table.Column<string>(type: "text", nullable: true),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "job_execution_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    Message = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ResponseBody = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_execution_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_job_execution_logs_job_definitions_JobDefinitionId",
                        column: x => x.JobDefinitionId,
                        principalTable: "job_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_job_definitions_Id",
                table: "job_definitions",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_job_execution_logs_JobCode",
                table: "job_execution_logs",
                column: "JobCode");

            migrationBuilder.CreateIndex(
                name: "IX_job_execution_logs_JobDefinitionId",
                table: "job_execution_logs",
                column: "JobDefinitionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "job_execution_logs");

            migrationBuilder.DropTable(
                name: "job_definitions");
        }
    }
}
