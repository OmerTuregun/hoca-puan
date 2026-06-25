using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using HocaPuan.Data;

#nullable disable

namespace HocaPuan.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260625120000_AddSearchNameIndexes")]
    public class AddSearchNameIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Professors_FirstName",
                table: "Professors",
                column: "FirstName");

            migrationBuilder.CreateIndex(
                name: "IX_Professors_LastName",
                table: "Professors",
                column: "LastName");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_Name",
                table: "Departments",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Professors_FirstName",
                table: "Professors");

            migrationBuilder.DropIndex(
                name: "IX_Professors_LastName",
                table: "Professors");

            migrationBuilder.DropIndex(
                name: "IX_Departments_Name",
                table: "Departments");
        }
    }
}
