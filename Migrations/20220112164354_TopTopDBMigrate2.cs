using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TopTopServer.Migrations
{
    public partial class TopTopDBMigrate2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "Video",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Video",
                table: "Comment",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "User",
                table: "Comment",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IsOwner",
                table: "Comment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Comment",
                table: "Comment",
                columns: new[] { "User", "Video", "CommentTime" });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotiTo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NotiFrom = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    NotiTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NotiContent = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => new { x.NotiTo, x.NotiTime, x.NotiFrom });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Comment",
                table: "Comment");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "Video");

            migrationBuilder.DropColumn(
                name: "IsOwner",
                table: "Comment");

            migrationBuilder.AlterColumn<string>(
                name: "Video",
                table: "Comment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "User",
                table: "Comment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
