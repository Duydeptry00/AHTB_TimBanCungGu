using Microsoft.EntityFrameworkCore.Migrations;

namespace AHTB_TimBanCungGu_API.Migrations
{
    public partial class updatephim : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DangPhim",
                table: "Phim",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DangPhim",
                table: "Phim");
        }
    }
}
