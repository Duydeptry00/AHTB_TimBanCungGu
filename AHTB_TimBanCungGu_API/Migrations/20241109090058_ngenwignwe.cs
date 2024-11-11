using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AHTB_TimBanCungGu_API.Migrations
{
    public partial class ngenwignwe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayMoKhoa",
                table: "Users",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "DiaChi",
                table: "ThongTinCN",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UuDai",
                columns: table => new
                {
                    IdUuDai = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenUuDai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayUuDai = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhanTram = table.Column<int>(type: "int", nullable: false),
                    Hinh = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UuDai", x => x.IdUuDai);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UuDai");

            migrationBuilder.DropColumn(
                name: "DiaChi",
                table: "ThongTinCN");

            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayMoKhoa",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
