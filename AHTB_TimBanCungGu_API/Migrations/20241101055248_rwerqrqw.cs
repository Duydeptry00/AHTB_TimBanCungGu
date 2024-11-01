using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AHTB_TimBanCungGu_API.Migrations
{
    public partial class rwerqrqw : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LichSuLyDoKhoa",
                table: "QuanLyNguoiDung",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LichSuMoKhoa",
                table: "QuanLyNguoiDung",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LichSuLyDoKhoa",
                table: "QuanLyNguoiDung");

            migrationBuilder.DropColumn(
                name: "LichSuMoKhoa",
                table: "QuanLyNguoiDung");
        }
    }
}
