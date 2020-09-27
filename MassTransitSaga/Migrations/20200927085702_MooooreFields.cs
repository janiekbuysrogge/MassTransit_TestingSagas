using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MassTransitSaga.Migrations
{
    public partial class MooooreFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalDate",
                table: "VergunningState",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmailId",
                table: "VergunningState",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GenerationDate",
                table: "VergunningState",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SendDate",
                table: "VergunningState",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalDate",
                table: "VergunningState");

            migrationBuilder.DropColumn(
                name: "EmailId",
                table: "VergunningState");

            migrationBuilder.DropColumn(
                name: "GenerationDate",
                table: "VergunningState");

            migrationBuilder.DropColumn(
                name: "SendDate",
                table: "VergunningState");
        }
    }
}
