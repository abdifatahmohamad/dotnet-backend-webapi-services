using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalSystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAccountFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSelfRegistered",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemAccount",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAtUtc", "IsSelfRegistered", "IsSystemAccount", "UpdatedAtUtc" },
                values: new object[] { new DateTime(2025, 12, 14, 23, 41, 58, 18, DateTimeKind.Utc).AddTicks(8960), false, true, new DateTime(2025, 12, 14, 23, 41, 58, 18, DateTimeKind.Utc).AddTicks(8960) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSelfRegistered",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsSystemAccount",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAtUtc", "UpdatedAtUtc" },
                values: new object[] { new DateTime(2025, 12, 14, 5, 18, 7, 275, DateTimeKind.Utc).AddTicks(3640), new DateTime(2025, 12, 14, 5, 18, 7, 275, DateTimeKind.Utc).AddTicks(3640) });
        }
    }
}
