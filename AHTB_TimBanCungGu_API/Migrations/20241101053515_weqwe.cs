using Microsoft.EntityFrameworkCore.Migrations;

namespace AHTB_TimBanCungGu_API.Migrations
{
    public partial class weqwe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LyDoKhoa",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LyDoKhoa",
                table: "Users");
        }
    }
}
