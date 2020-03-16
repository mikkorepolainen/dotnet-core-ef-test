using Microsoft.EntityFrameworkCore.Migrations;

namespace Data.Migrations
{
    public partial class OtherValue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OtherValue",
                table: "Table",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherValue",
                table: "Table");
        }
    }
}
