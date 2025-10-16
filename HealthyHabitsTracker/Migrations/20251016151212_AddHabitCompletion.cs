using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthyHabitsTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddHabitCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HabitCompletions",
                columns: table => new
                {
                    CompletionId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HabitId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HabitCompletions", x => x.CompletionId);
                    table.ForeignKey(
                        name: "FK_HabitCompletions_Habits_HabitId",
                        column: x => x.HabitId,
                        principalTable: "Habits",
                        principalColumn: "HabitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HabitCompletions_HabitId_CompletionDate",
                table: "HabitCompletions",
                columns: new[] { "HabitId", "CompletionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_HabitCompletions_UserId_CompletionDate",
                table: "HabitCompletions",
                columns: new[] { "UserId", "CompletionDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HabitCompletions");
        }
    }
}
