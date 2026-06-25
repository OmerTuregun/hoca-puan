using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HocaPuan.Data.Migrations;

public partial class AddUserAccountLockout : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "FailedLoginAttempts",
            table: "Users",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<DateTime>(
            name: "LockoutEnd",
            table: "Users",
            type: "timestamp with time zone",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "FailedLoginAttempts",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "LockoutEnd",
            table: "Users");
    }
}
