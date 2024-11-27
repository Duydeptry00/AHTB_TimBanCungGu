using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AHTB_TimBanCungGu_API.Migrations
{
    public partial class cuong : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Quyen",
                columns: table => new
                {
                    IDRole = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Module = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Add = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Update = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Delete = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenRole = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quyen", x => x.IDRole);
                });

            migrationBuilder.CreateTable(
                name: "TheLoai",
                columns: table => new
                {
                    IdTheLoai = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenTheLoai = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TheLoai", x => x.IdTheLoai);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UsID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayMoKhoa = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LyDoKhoa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UsID);
                });

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

            migrationBuilder.CreateTable(
                name: "BaoCaoNguoiDung",
                columns: table => new
                {
                    IDReport = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiBaoCao = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DoiTuongBaoCao = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LyDoBaoCao = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayBaoCao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaoCaoNguoiDung", x => x.IDReport);
                    table.ForeignKey(
                        name: "FK_BaoCaoNguoiDung_Users_DoiTuongBaoCao",
                        column: x => x.DoiTuongBaoCao,
                        principalTable: "Users",
                        principalColumn: "UsID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BaoCaoNguoiDung_Users_NguoiBaoCao",
                        column: x => x.NguoiBaoCao,
                        principalTable: "Users",
                        principalColumn: "UsID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CungGu",
                columns: table => new
                {
                    IdCungGu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiDungTim = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DoiTuongDuocTim = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ThoiGianTim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HuongVuot = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CungGu", x => x.IdCungGu);
                    table.ForeignKey(
                        name: "FK_CungGu_Users_DoiTuongDuocTim",
                        column: x => x.DoiTuongDuocTim,
                        principalTable: "Users",
                        principalColumn: "UsID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CungGu_Users_NguoiDungTim",
                        column: x => x.NguoiDungTim,
                        principalTable: "Users",
                        principalColumn: "UsID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HoaDon",
                columns: table => new
                {
                    IDHoaDon = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiMua = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GoiPremium = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayHetHan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TongTien = table.Column<double>(type: "float", nullable: false),
                    PhuongThucThanhToan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayThanhToan = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDon", x => x.IDHoaDon);
                    table.ForeignKey(
                        name: "FK_HoaDon_Users_NguoiMua",
                        column: x => x.NguoiMua,
                        principalTable: "Users",
                        principalColumn: "UsID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Phim",
                columns: table => new
                {
                    IDPhim = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenPhim = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DienVien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TheLoaiPhim = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NgayPhatHanh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DanhGia = table.Column<double>(type: "float", nullable: false),
                    TrailerURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDungPremium = table.Column<bool>(type: "bit", nullable: false),
                    SourcePhim = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DangPhim = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IDAdmin = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phim", x => x.IDPhim);
                    table.ForeignKey(
                        name: "FK_Phim_TheLoai_TheLoaiPhim",
                        column: x => x.TheLoaiPhim,
                        principalTable: "TheLoai",
                        principalColumn: "IdTheLoai",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Phim_Users_IDAdmin",
                        column: x => x.IDAdmin,
                        principalTable: "Users",
                        principalColumn: "UsID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuanLyNguoiDung",
                columns: table => new
                {
                    IDQLND = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NguoiDungID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ThaoTac = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MocThoiGian = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LichSuMoKhoa = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LichSuLyDoKhoa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuanLyNguoiDung", x => x.IDQLND);
                    table.ForeignKey(
                        name: "FK_QuanLyNguoiDung_Users_AdminID",
                        column: x => x.AdminID,
                        principalTable: "Users",
                        principalColumn: "UsID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuanLyNguoiDung_Users_NguoiDungID",
                        column: x => x.NguoiDungID,
                        principalTable: "Users",
                        principalColumn: "UsID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    IDRole_US = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IDRole = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.IDRole_US);
                    table.ForeignKey(
                        name: "FK_Role_Quyen_IDRole",
                        column: x => x.IDRole,
                        principalTable: "Quyen",
                        principalColumn: "IDRole",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Role_Users_UsID",
                        column: x => x.UsID,
                        principalTable: "Users",
                        principalColumn: "UsID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThongTinCN",
                columns: table => new
                {
                    IDProfile = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GioiTinh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPremium = table.Column<bool>(type: "bit", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongTinCN", x => x.IDProfile);
                    table.ForeignKey(
                        name: "FK_ThongTinCN_Users_UsID",
                        column: x => x.UsID,
                        principalTable: "Users",
                        principalColumn: "UsID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichSuXem",
                columns: table => new
                {
                    IDLSX = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiDungXem = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PhimDaXem = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ThoiGianXem = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuXem", x => x.IDLSX);
                    table.ForeignKey(
                        name: "FK_LichSuXem_Phim_PhimDaXem",
                        column: x => x.PhimDaXem,
                        principalTable: "Phim",
                        principalColumn: "IDPhim",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuXem_Users_NguoiDungXem",
                        column: x => x.NguoiDungXem,
                        principalTable: "Users",
                        principalColumn: "UsID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Phan",
                columns: table => new
                {
                    IDPhan = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SoPhan = table.Column<int>(type: "int", nullable: false),
                    NgayCongChieu = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoLuongTap = table.Column<int>(type: "int", nullable: false),
                    PhimID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phan", x => x.IDPhan);
                    table.ForeignKey(
                        name: "FK_Phan_Phim_PhimID",
                        column: x => x.PhimID,
                        principalTable: "Phim",
                        principalColumn: "IDPhim",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhimYeuThich",
                columns: table => new
                {
                    IdYeuThich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiDungYT = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PhimYT = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhimYeuThich", x => x.IdYeuThich);
                    table.ForeignKey(
                        name: "FK_PhimYeuThich_Phim_PhimYT",
                        column: x => x.PhimYT,
                        principalTable: "Phim",
                        principalColumn: "IDPhim",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhimYeuThich_Users_NguoiDungYT",
                        column: x => x.NguoiDungYT,
                        principalTable: "Users",
                        principalColumn: "UsID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AnhCaNhan",
                columns: table => new
                {
                    IDAnhCN = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IDProfile = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnhCaNhan", x => x.IDAnhCN);
                    table.ForeignKey(
                        name: "FK_AnhCaNhan_ThongTinCN_IDProfile",
                        column: x => x.IDProfile,
                        principalTable: "ThongTinCN",
                        principalColumn: "IDProfile",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tap",
                columns: table => new
                {
                    IDTap = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SoTap = table.Column<int>(type: "int", nullable: false),
                    PhanPhim = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tap", x => x.IDTap);
                    table.ForeignKey(
                        name: "FK_Tap_Phan_PhanPhim",
                        column: x => x.PhanPhim,
                        principalTable: "Phan",
                        principalColumn: "IDPhan",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnhCaNhan_IDProfile",
                table: "AnhCaNhan",
                column: "IDProfile");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoNguoiDung_DoiTuongBaoCao",
                table: "BaoCaoNguoiDung",
                column: "DoiTuongBaoCao");

            migrationBuilder.CreateIndex(
                name: "IX_BaoCaoNguoiDung_NguoiBaoCao",
                table: "BaoCaoNguoiDung",
                column: "NguoiBaoCao");

            migrationBuilder.CreateIndex(
                name: "IX_CungGu_DoiTuongDuocTim",
                table: "CungGu",
                column: "DoiTuongDuocTim");

            migrationBuilder.CreateIndex(
                name: "IX_CungGu_NguoiDungTim",
                table: "CungGu",
                column: "NguoiDungTim");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_NguoiMua",
                table: "HoaDon",
                column: "NguoiMua");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuXem_NguoiDungXem",
                table: "LichSuXem",
                column: "NguoiDungXem");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuXem_PhimDaXem",
                table: "LichSuXem",
                column: "PhimDaXem");

            migrationBuilder.CreateIndex(
                name: "IX_Phan_PhimID",
                table: "Phan",
                column: "PhimID");

            migrationBuilder.CreateIndex(
                name: "IX_Phim_IDAdmin",
                table: "Phim",
                column: "IDAdmin");

            migrationBuilder.CreateIndex(
                name: "IX_Phim_TheLoaiPhim",
                table: "Phim",
                column: "TheLoaiPhim");

            migrationBuilder.CreateIndex(
                name: "IX_PhimYeuThich_NguoiDungYT",
                table: "PhimYeuThich",
                column: "NguoiDungYT");

            migrationBuilder.CreateIndex(
                name: "IX_PhimYeuThich_PhimYT",
                table: "PhimYeuThich",
                column: "PhimYT");

            migrationBuilder.CreateIndex(
                name: "IX_QuanLyNguoiDung_AdminID",
                table: "QuanLyNguoiDung",
                column: "AdminID");

            migrationBuilder.CreateIndex(
                name: "IX_QuanLyNguoiDung_NguoiDungID",
                table: "QuanLyNguoiDung",
                column: "NguoiDungID");

            migrationBuilder.CreateIndex(
                name: "IX_Role_IDRole",
                table: "Role",
                column: "IDRole");

            migrationBuilder.CreateIndex(
                name: "IX_Role_UsID",
                table: "Role",
                column: "UsID");

            migrationBuilder.CreateIndex(
                name: "IX_Tap_PhanPhim",
                table: "Tap",
                column: "PhanPhim");

            migrationBuilder.CreateIndex(
                name: "IX_ThongTinCN_UsID",
                table: "ThongTinCN",
                column: "UsID",
                unique: true,
                filter: "[UsID] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnhCaNhan");

            migrationBuilder.DropTable(
                name: "BaoCaoNguoiDung");

            migrationBuilder.DropTable(
                name: "CungGu");

            migrationBuilder.DropTable(
                name: "HoaDon");

            migrationBuilder.DropTable(
                name: "LichSuXem");

            migrationBuilder.DropTable(
                name: "PhimYeuThich");

            migrationBuilder.DropTable(
                name: "QuanLyNguoiDung");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Tap");

            migrationBuilder.DropTable(
                name: "UuDai");

            migrationBuilder.DropTable(
                name: "ThongTinCN");

            migrationBuilder.DropTable(
                name: "Quyen");

            migrationBuilder.DropTable(
                name: "Phan");

            migrationBuilder.DropTable(
                name: "Phim");

            migrationBuilder.DropTable(
                name: "TheLoai");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
