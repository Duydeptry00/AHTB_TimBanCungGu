﻿// <auto-generated />
using System;
using AHTB_TimBanCungGu_API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AHTB_TimBanCungGu_API.Migrations
{
    [DbContext(typeof(DBAHTBContext))]
    [Migration("20241119043549_ngewnget")]
    partial class ngewnget
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.17")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.AnhCaNhan", b =>
                {
                    b.Property<int>("IDAnhCN")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("HinhAnh")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("IDProfile")
                        .HasColumnType("int");

                    b.HasKey("IDAnhCN");

                    b.HasIndex("IDProfile");

                    b.ToTable("AnhCaNhan");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.BaoCaoNguoiDung", b =>
                {
                    b.Property<int>("IDReport")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("DoiTuongBaoCao")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LyDoBaoCao")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("NgayBaoCao")
                        .HasColumnType("datetime2");

                    b.Property<string>("NguoiBaoCao")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("TrangThai")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IDReport");

                    b.HasIndex("DoiTuongBaoCao");

                    b.HasIndex("NguoiBaoCao");

                    b.ToTable("BaoCaoNguoiDung");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.CungGu", b =>
                {
                    b.Property<int>("IdCungGu")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("DoiTuongDuocTim")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("HuongVuot")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NguoiDungTim")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("ThoiGianTim")
                        .HasColumnType("datetime2");

                    b.HasKey("IdCungGu");

                    b.HasIndex("DoiTuongDuocTim");

                    b.HasIndex("NguoiDungTim");

                    b.ToTable("CungGu");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.HoaDon", b =>
                {
                    b.Property<int>("IDHoaDon")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("GoiPremium")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("NgayHetHan")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("NgayThanhToan")
                        .HasColumnType("datetime2");

                    b.Property<string>("NguoiMua")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("PhuongThucThanhToan")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("TongTien")
                        .HasColumnType("float");

                    b.Property<string>("TrangThai")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IDHoaDon");

                    b.HasIndex("NguoiMua");

                    b.ToTable("HoaDon");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.LichSuXem", b =>
                {
                    b.Property<int>("IDLSX")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("NguoiDungXem")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("PhimDaXem")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("ThoiGianXem")
                        .HasColumnType("datetime2");

                    b.HasKey("IDLSX");

                    b.HasIndex("NguoiDungXem");

                    b.HasIndex("PhimDaXem");

                    b.ToTable("LichSuXem");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.Phan", b =>
                {
                    b.Property<string>("IDPhan")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("NgayCongChieu")
                        .HasColumnType("datetime2");

                    b.Property<string>("PhimID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("SoLuongTap")
                        .HasColumnType("int");

                    b.Property<int>("SoPhan")
                        .HasColumnType("int");

                    b.HasKey("IDPhan");

                    b.HasIndex("PhimID");

                    b.ToTable("Phan");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.Phim", b =>
                {
                    b.Property<string>("IDPhim")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("DangPhim")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("DanhGia")
                        .HasColumnType("float");

                    b.Property<string>("DienVien")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HinhAnh")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("IDAdmin")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("MoTa")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("NgayCapNhat")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("NgayPhatHanh")
                        .HasColumnType("datetime2");

                    b.Property<bool>("NoiDungPremium")
                        .HasColumnType("bit");

                    b.Property<string>("SourcePhim")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenPhim")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TheLoaiPhim")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("TrailerURL")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TrangThai")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IDPhim");

                    b.HasIndex("IDAdmin");

                    b.HasIndex("TheLoaiPhim");

                    b.ToTable("Phim");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.PhimYeuThich", b =>
                {
                    b.Property<int>("IdYeuThich")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("NguoiDungYT")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("PhimYT")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("IdYeuThich");

                    b.HasIndex("NguoiDungYT");

                    b.HasIndex("PhimYT");

                    b.ToTable("PhimYeuThich");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.QuanLyNguoiDung", b =>
                {
                    b.Property<int>("IDQLND")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AdminID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LichSuLyDoKhoa")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("LichSuMoKhoa")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("MocThoiGian")
                        .HasColumnType("datetime2");

                    b.Property<string>("NguoiDungID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ThaoTac")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IDQLND");

                    b.HasIndex("AdminID");

                    b.HasIndex("NguoiDungID");

                    b.ToTable("QuanLyNguoiDung");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.Role", b =>
                {
                    b.Property<int>("IDRole")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Add")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Delete")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Module")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ReviewDetails")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Update")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IDRole");

                    b.ToTable("Quyen");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.Tap", b =>
                {
                    b.Property<string>("IDTap")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("PhanPhim")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("SoTap")
                        .HasColumnType("int");

                    b.HasKey("IDTap");

                    b.HasIndex("PhanPhim");

                    b.ToTable("Tap");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.TheLoai", b =>
                {
                    b.Property<string>("IdTheLoai")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("TenTheLoai")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdTheLoai");

                    b.ToTable("TheLoai");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.ThongTinCaNhan", b =>
                {
                    b.Property<int>("IDProfile")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("DiaChi")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("GioiTinh")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HoTen")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsPremium")
                        .HasColumnType("bit");

                    b.Property<string>("MoTa")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("NgaySinh")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("NgayTao")
                        .HasColumnType("datetime2");

                    b.Property<string>("SoDienThoai")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TrangThai")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UsID")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("IDProfile");

                    b.HasIndex("UsID")
                        .IsUnique()
                        .HasFilter("[UsID] IS NOT NULL");

                    b.ToTable("ThongTinCN");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.User", b =>
                {
                    b.Property<string>("UsID")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LyDoKhoa")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("NgayMoKhoa")
                        .HasColumnType("datetime2");

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TrangThai")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UsID");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.User_Role", b =>
                {
                    b.Property<int>("IDRole_US")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("IDRole")
                        .HasColumnType("int");

                    b.Property<string>("TenRole")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UsID")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("IDRole_US");

                    b.HasIndex("IDRole");

                    b.HasIndex("UsID");

                    b.ToTable("Role");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.UuDai", b =>
                {
                    b.Property<string>("IdUuDai")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Hinh")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("NgayUuDai")
                        .HasColumnType("datetime2");

                    b.Property<int>("PhanTram")
                        .HasColumnType("int");

                    b.Property<string>("TenUuDai")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("IdUuDai");

                    b.ToTable("UuDai");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.AnhCaNhan", b =>
                {
                    b.HasOne("AHTB_TimBanCungGu_API.Models.ThongTinCaNhan", "ThongTinCN")
                        .WithMany("AnhCaNhan")
                        .HasForeignKey("IDProfile")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ThongTinCN");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.BaoCaoNguoiDung", b =>
                {
                    b.HasOne("AHTB_TimBanCungGu_API.Models.User", "DoiTuongBaoCaoUser")
                        .WithMany("ReportsDoiTuongBaoCao")
                        .HasForeignKey("DoiTuongBaoCao");

                    b.HasOne("AHTB_TimBanCungGu_API.Models.User", "NguoiBaoCaoUser")
                        .WithMany("ReportsNguoiBaoCao")
                        .HasForeignKey("NguoiBaoCao");

                    b.Navigation("DoiTuongBaoCaoUser");

                    b.Navigation("NguoiBaoCaoUser");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.CungGu", b =>
                {
                    b.HasOne("AHTB_TimBanCungGu_API.Models.User", "DoiTuongDuocTimUser")
                        .WithMany("CungGuDoiTuongDuocTim")
                        .HasForeignKey("DoiTuongDuocTim");

                    b.HasOne("AHTB_TimBanCungGu_API.Models.User", "NguoiDungTimUser")
                        .WithMany("CungGuNguoiDungTim")
                        .HasForeignKey("NguoiDungTim");

                    b.Navigation("DoiTuongDuocTimUser");

                    b.Navigation("NguoiDungTimUser");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.HoaDon", b =>
                {
                    b.HasOne("AHTB_TimBanCungGu_API.Models.User", "User")
                        .WithMany("HoaDon")
                        .HasForeignKey("NguoiMua");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.LichSuXem", b =>
                {
                    b.HasOne("AHTB_TimBanCungGu_API.Models.User", "User")
                        .WithMany("LichSuXem")
                        .HasForeignKey("NguoiDungXem");

                    b.HasOne("AHTB_TimBanCungGu_API.Models.Phim", "Phim")
                        .WithMany("LichSuXem")
                        .HasForeignKey("PhimDaXem");

                    b.Navigation("Phim");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.Phan", b =>
                {
                    b.HasOne("AHTB_TimBanCungGu_API.Models.Phim", "Phim")
                        .WithMany("Phan")
                        .HasForeignKey("PhimID");

                    b.Navigation("Phim");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.Phim", b =>
                {
                    b.HasOne("AHTB_TimBanCungGu_API.Models.User", "User")
                        .WithMany("Phim")
                        .HasForeignKey("IDAdmin");

                    b.HasOne("AHTB_TimBanCungGu_API.Models.TheLoai", "TheLoai")
                        .WithMany("Phim")
                        .HasForeignKey("TheLoaiPhim");

                    b.Navigation("TheLoai");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.PhimYeuThich", b =>
                {
                    b.HasOne("AHTB_TimBanCungGu_API.Models.User", "User")
                        .WithMany("PhimYeuThich")
                        .HasForeignKey("NguoiDungYT");

                    b.HasOne("AHTB_TimBanCungGu_API.Models.Phim", "Phim")
                        .WithMany("PhimYeuThich")
                        .HasForeignKey("PhimYT");

                    b.Navigation("Phim");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.QuanLyNguoiDung", b =>
                {
                    b.HasOne("AHTB_TimBanCungGu_API.Models.User", "AdminUser")
                        .WithMany("QuanLyNguoiDungAdmin")
                        .HasForeignKey("AdminID");

                    b.HasOne("AHTB_TimBanCungGu_API.Models.User", "NguoiDungUser")
                        .WithMany("QuanLyNguoiDungNguoiDung")
                        .HasForeignKey("NguoiDungID");

                    b.Navigation("AdminUser");

                    b.Navigation("NguoiDungUser");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.Tap", b =>
                {
                    b.HasOne("AHTB_TimBanCungGu_API.Models.Phan", "Phan")
                        .WithMany("Tap")
                        .HasForeignKey("PhanPhim");

                    b.Navigation("Phan");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.ThongTinCaNhan", b =>
                {
                    b.HasOne("AHTB_TimBanCungGu_API.Models.User", "User")
                        .WithOne("ThongTinCN")
                        .HasForeignKey("AHTB_TimBanCungGu_API.Models.ThongTinCaNhan", "UsID");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.User_Role", b =>
                {
                    b.HasOne("AHTB_TimBanCungGu_API.Models.Role", "Role")
                        .WithMany("User_Role")
                        .HasForeignKey("IDRole")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AHTB_TimBanCungGu_API.Models.User", "User")
                        .WithMany("User_Role")
                        .HasForeignKey("UsID");

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.Phan", b =>
                {
                    b.Navigation("Tap");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.Phim", b =>
                {
                    b.Navigation("LichSuXem");

                    b.Navigation("Phan");

                    b.Navigation("PhimYeuThich");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.Role", b =>
                {
                    b.Navigation("User_Role");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.TheLoai", b =>
                {
                    b.Navigation("Phim");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.ThongTinCaNhan", b =>
                {
                    b.Navigation("AnhCaNhan");
                });

            modelBuilder.Entity("AHTB_TimBanCungGu_API.Models.User", b =>
                {
                    b.Navigation("CungGuDoiTuongDuocTim");

                    b.Navigation("CungGuNguoiDungTim");

                    b.Navigation("HoaDon");

                    b.Navigation("LichSuXem");

                    b.Navigation("Phim");

                    b.Navigation("PhimYeuThich");

                    b.Navigation("QuanLyNguoiDungAdmin");

                    b.Navigation("QuanLyNguoiDungNguoiDung");

                    b.Navigation("ReportsDoiTuongBaoCao");

                    b.Navigation("ReportsNguoiBaoCao");

                    b.Navigation("ThongTinCN");

                    b.Navigation("User_Role");
                });
#pragma warning restore 612, 618
        }
    }
}
