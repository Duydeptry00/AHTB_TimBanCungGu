using Microsoft.EntityFrameworkCore.Migrations;

namespace AHTB_TimBanCungGu_API.Migrations
{
    public partial class niowentiewt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TenRole",
                table: "Role",
                newName: "TrangThai");

            migrationBuilder.AddColumn<string>(
                name: "TenRole",
                table: "Quyen",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenRole",
                table: "Quyen");

            migrationBuilder.RenameColumn(
                name: "TrangThai",
                table: "Role",
                newName: "TenRole");
        }
    }
}
