using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MembershipSystemAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddLastCheckedForConnectionToMembershipCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastCheckedForConnection",
                table: "MembershipCards",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastCheckedForConnection",
                table: "MembershipCards");
        }
    }
}
