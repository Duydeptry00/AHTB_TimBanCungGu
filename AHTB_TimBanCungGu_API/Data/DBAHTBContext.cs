using AHTB_TimBanCungGu_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;

namespace AHTB_TimBanCungGu_API.Data
{
    public class DBAHTBContext : DbContext
    {
        public DbSet<AnhCaNhan> AnhCaNhan { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<BaoCaoNguoiDung> BaoCaoNguoiDung { get; set; }
        public DbSet<CungGu> CungGu { get; set; }
        public DbSet<LichSuXem> LichSuXem { get; set; }
        public DbSet<HoaDon> HoaDon { get; set; }
        public DbSet<Phim> Phim { get; set; }
        public DbSet<Phan> Phan { get; set; }
        public DbSet<Tap> Tap { get; set; }
        public DbSet<PhimYeuThich> PhimYeuThich { get; set; }
        public DbSet<QuanLyNguoiDung> QuanLyNguoiDung { get; set; }
        public DbSet<Role> Quyen { get; set; }
        public DbSet<TheLoai> TheLoai { get; set; }
        public DbSet<User_Role> Role { get; set; }
        public DbSet<ThongTinCaNhan> ThongTinCN { get; set; }
        public DBAHTBContext(DbContextOptions<DBAHTBContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AnhCaNhan>().HasKey(p => p.IDAnhCN);
            modelBuilder.Entity<User>().HasKey(p => p.UsID);
            modelBuilder.Entity<BaoCaoNguoiDung>().HasKey(p => p.IDReport);
            modelBuilder.Entity<CungGu>().HasKey(c => c.IdCungGu);
            modelBuilder.Entity<LichSuXem>().HasKey(p => p.IDLSX);
            modelBuilder.Entity<HoaDon>().HasKey(c => c.IDHoaDon);
            modelBuilder.Entity<Phim>().HasKey(p => p.IDPhim);
            modelBuilder.Entity<Phan>().HasKey(c => c.IDPhan);
            modelBuilder.Entity<Tap>().HasKey(p => p.IDTap);
            modelBuilder.Entity<PhimYeuThich>().HasKey(c => c.IdYeuThich);
            modelBuilder.Entity<QuanLyNguoiDung>().HasKey(p => p.IDQLND);
            modelBuilder.Entity<Role>().HasKey(c => c.IDRole);
            modelBuilder.Entity<TheLoai>().HasKey(p => p.IdTheLoai);
            modelBuilder.Entity<User_Role>().HasKey(p => p.IDRole_US);
            modelBuilder.Entity<ThongTinCaNhan>().HasKey(p => p.IDProfile);

            modelBuilder.Entity<AnhCaNhan>()
                .HasOne(p => p.ThongTinCN)
                .WithMany(c => c.AnhCaNhan)
                .HasForeignKey(p => p.IDProfile);
            modelBuilder.Entity<HoaDon>()
             .HasOne(p => p.User)
             .WithMany(c => c.HoaDon)
             .HasForeignKey(p => p.NguoiMua);
            modelBuilder.Entity<Phim>()
              .HasOne(p => p.User)
              .WithMany(c => c.Phim)
              .HasForeignKey(p => p.IDAdmin);
            modelBuilder.Entity<BaoCaoNguoiDung>()
              .HasOne(p => p.NguoiBaoCaoUser)
              .WithMany(c => c.ReportsNguoiBaoCao)
              .HasForeignKey(p => p.NguoiBaoCao);
            modelBuilder.Entity<BaoCaoNguoiDung>()
             .HasOne(p => p.DoiTuongBaoCaoUser)
             .WithMany(c => c.ReportsDoiTuongBaoCao)
             .HasForeignKey(p => p.DoiTuongBaoCao);
            modelBuilder.Entity<CungGu>()
          .HasOne(p => p.NguoiDungTimUser)
          .WithMany(c => c.CungGuNguoiDungTim)
          .HasForeignKey(p => p.NguoiDungTim);
            modelBuilder.Entity<CungGu>()
          .HasOne(p => p.DoiTuongDuocTimUser)
          .WithMany(c => c.CungGuDoiTuongDuocTim)
          .HasForeignKey(p => p.DoiTuongDuocTim);
            modelBuilder.Entity<LichSuXem>()
              .HasOne(p => p.User)
              .WithMany(c => c.LichSuXem)
              .HasForeignKey(p => p.NguoiDungXem);
            modelBuilder.Entity<LichSuXem>()
              .HasOne(p => p.Phim)
              .WithMany(c => c.LichSuXem)
              .HasForeignKey(p => p.PhimDaXem);
            modelBuilder.Entity<Phan>()
              .HasOne(p => p.Phim)
              .WithMany(c => c.Phan)
              .HasForeignKey(p => p.PhimID);
            modelBuilder.Entity<Phim>()
           .HasOne(p => p.TheLoai)
           .WithMany(c => c.Phim)
           .HasForeignKey(p => p.TheLoaiPhim);
            modelBuilder.Entity<PhimYeuThich>()
              .HasOne(p => p.User)
              .WithMany(c => c.PhimYeuThich)
              .HasForeignKey(p => p.NguoiDungYT);
            modelBuilder.Entity<PhimYeuThich>()
              .HasOne(p => p.Phim)
              .WithMany(c => c.PhimYeuThich)
              .HasForeignKey(p => p.PhimYT);
            modelBuilder.Entity<QuanLyNguoiDung>()
              .HasOne(p => p.NguoiDungUser)
              .WithMany(c => c.QuanLyNguoiDungNguoiDung)
              .HasForeignKey(p => p.NguoiDungID);
            modelBuilder.Entity<QuanLyNguoiDung>()
              .HasOne(p => p.AdminUser)
              .WithMany(c => c.QuanLyNguoiDungAdmin)
              .HasForeignKey(p => p.AdminID);
            modelBuilder.Entity<User_Role>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.User_Role)
            .HasForeignKey(ur => ur.UsID);
            modelBuilder.Entity<User_Role>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.User_Role)
                .HasForeignKey(ur => ur.IDRole);
            modelBuilder.Entity<Tap>()
              .HasOne(p => p.Phan)
              .WithMany(c => c.Tap)
              .HasForeignKey(p => p.PhanPhim);
            modelBuilder.Entity<ThongTinCaNhan>()
           .HasOne<User>(p => p.User)
           .WithOne(c => c.ThongTinCN);

        }
    }
}
