﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class ThirdValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThirdValue",
                table: "Table",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThirdValue",
                table: "Table");
        }
    }
}
