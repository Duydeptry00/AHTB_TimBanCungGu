using AHTB_TimBanCungGu_API.Data;
using AHTB_TimBanCungGu_API.Models;
using AHTB_TimBanCungGu_MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using System;

namespace AHTB_TimBanCungGu_MVC.Areas.Admin.Controllers
{
    public class DoanhThus : Controller
    {
        private readonly DBAHTBContext _context;

        public DoanhThus(DBAHTBContext context)
        {
            _context = context;
        }

        [Area("Admin")]
        public IActionResult Index()
        {
            // Nhóm doanh thu theo tháng cho từng năm
            var monthlyTotals = _context.HoaDon
                .GroupBy(h => new { Year = h.NgayThanhToan.Year, Month = h.NgayThanhToan.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalTongTien = g.Sum(h => h.TongTien)
                })
                .OrderBy(g => g.Year)
                .ThenBy(g => g.Month)
                .ToList();

            // Nhóm doanh thu theo năm
            var yearlyTotals = _context.HoaDon
                .GroupBy(h => h.NgayThanhToan.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    TotalTongTien = g.Sum(h => h.TongTien)
                })
                .OrderBy(g => g.Year)
                .ToList();

            // Lấy danh sách các năm duy nhất
            var years = _context.HoaDon
                .Select(h => h.NgayThanhToan.Year)
                .Distinct()
                .OrderBy(year => year)
                .ToList();

            // Tìm năm gần nhất với năm hiện tại
            var currentYear = DateTime.Now.Year;
            var latestYear = years
                .Where(y => y <= currentYear) // Lọc những năm không lớn hơn năm hiện tại
                .OrderByDescending(y => y) // Sắp xếp theo thứ tự giảm dần
                .FirstOrDefault(); // Lấy năm gần nhất

            // Lọc ra các năm từ năm gần nhất về trước
            var yearsToDisplay = years
                .Where(y => y <= latestYear)
                .OrderByDescending(y => y) // Sắp xếp theo thứ tự giảm dần
                .ToList();

            // Truyền kết quả vào ViewBag để truy cập từ View
            ViewBag.MonthlyTotals = monthlyTotals;
            ViewBag.YearlyTotals = yearlyTotals;
            ViewBag.Years = yearsToDisplay; // Truyền danh sách năm cần hiển thị vào ViewBag

            return View();
        }
    }
}
