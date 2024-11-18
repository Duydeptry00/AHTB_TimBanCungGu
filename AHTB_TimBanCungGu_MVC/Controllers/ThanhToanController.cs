using AHTB_TimBanCungGu_API.Models; 
using AHTB_TimBanCungGu_API.ViewModels;
using Microsoft.AspNetCore.Mvc;
using AHTB_TimBanCungGu_MVC.Helpers;
using AHTB_TimBanCungGu_MVC.Services;
using System;
using Microsoft.Extensions.Logging;
using AHTB_TimBanCungGu_API.Data;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace AHTB_TimBanCungGu_MVC.Controllers
{
    public class ThanhToanController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IVnPayService _vnPayservice;
        private readonly DBAHTBContext _context;

        public ThanhToanController(ILogger<HomeController> logger, IVnPayService vnPayservice, DBAHTBContext context)
        {
            _logger = logger;
            _vnPayservice = vnPayservice;
            _context = context;
        }

        [HttpGet]
        public IActionResult Premium()
        {
            // Lấy JWT token từ Session
            var token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                return View();
            }
            else
            {
                // Nếu không có token, có thể chuyển đến trang đăng nhập
                ViewBag.Message = "Bạn chưa đăng nhập.";
                return RedirectToAction("Login", "LoginvsRegister");
            }
        }

        [HttpPost]
        public IActionResult Premium(int GiaGoi, int SoThang, string payment)
        {
            // Lưu số tiền và số tháng vào session trước khi xử lý thanh toán
            var amountBytes = System.Text.Encoding.UTF8.GetBytes(GiaGoi.ToString());
            HttpContext.Session.Set("Amount", amountBytes);

            var soThangBytes = System.Text.Encoding.UTF8.GetBytes(SoThang.ToString());
            HttpContext.Session.Set("SoThang", soThangBytes);

            if (payment == "VNPay")
            {
                var vnPayModel = new VnPaymentRequestModel
                {
                    Amount = GiaGoi,
                    CreatedDate = DateTime.Now,
                    OrderId = new Random().Next(1000, 100000)
                };
                return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
            }

            return RedirectToAction(nameof(Index));
        }


        public IActionResult PaymentSuccess()
        {
            // Lấy JWT token từ Session
            var token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                return View();
            }
            else
            {
                // Nếu không có token, có thể chuyển đến trang đăng nhập
                ViewBag.Message = "Bạn chưa đăng nhập.";
                return RedirectToAction("Login", "LoginvsRegister");
            }
        }

        public IActionResult PaymentFail()
        {
            // Lấy JWT token từ Session
            var token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                return View();
            }
            else
            {
                // Nếu không có token, có thể chuyển đến trang đăng nhập
                ViewBag.Message = "Bạn chưa đăng nhập.";
                return RedirectToAction("Login", "LoginvsRegister");
            }
        }

        public IActionResult PaymentCallBack()
        {
            var response = _vnPayservice.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VN Pay: {response?.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }

            // Lấy số tiền từ session
            if (!HttpContext.Session.TryGetValue("Amount", out var amountBytes) ||
                !double.TryParse(System.Text.Encoding.UTF8.GetString(amountBytes), out var amount))
            {
                TempData["Message"] = "Không tìm thấy số tiền thanh toán.";
                return RedirectToAction("PaymentFail");
            }

            // Lấy số tháng từ session
            if (!HttpContext.Session.TryGetValue("SoThang", out var soThangBytes) ||
                !int.TryParse(System.Text.Encoding.UTF8.GetString(soThangBytes), out var soThang))
            {
                TempData["Message"] = "Không tìm thấy thông tin số tháng.";
                return RedirectToAction("PaymentFail");
            }
            var user = _context.Users.FirstOrDefault(p => p.UserName == HttpContext.Session.GetString("TempUserName"));
            string userID = user.UsID;

            // Tạo mới hóa đơn với thông tin gói Premium và thời gian hết hạn dựa trên số tháng
            var hoaDon = new HoaDon
            {
                NguoiMua = userID,
                GoiPremium = $"Gói Premium {soThang} tháng",
                NgayHetHan = DateTime.Now.AddMonths(soThang), // Tính ngày hết hạn
                TongTien = amount,
                PhuongThucThanhToan = "VNPay",
                TrangThai = "Đã thanh toán",
                NgayThanhToan = DateTime.Now
            };

            // Lưu hóa đơn vào database...
            try
            {
                _context.HoaDon.Add(hoaDon);
                _context.SaveChanges();
                var ttcn = _context.ThongTinCN.FirstOrDefault(p => p.UsID == userID);

                ttcn.IsPremium = true;
              _context.SaveChangesAsync();
                return RedirectToAction("PaymentSuccess");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi lưu hóa đơn: {ex.Message}");
                return RedirectToAction("PaymentFail");
            }
        }

    }
}
