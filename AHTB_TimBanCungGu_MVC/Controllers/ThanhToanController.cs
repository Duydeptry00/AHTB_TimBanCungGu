using AHTB_TimBanCungGu_MVC.Models; 
using Microsoft.AspNetCore.Mvc;
using AHTB_TimBanCungGu_MVC.Helpers;
using AHTB_TimBanCungGu_MVC.Services;
using System;
using Microsoft.Extensions.Logging;
using AHTB_TimBanCungGu_API.Data;

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
            return View();
        }

        [HttpPost]
        public IActionResult Premium(int GiaGoi, string payment)
        {
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
            return View();
        }

        public IActionResult PaymentFail()
        {
            return View();
        }

        public IActionResult PaymentCallBack()
        {
            var response = _vnPayservice.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VN Pay: {response?.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }

            // Lấy số tiền từ phiên
            if (!HttpContext.Session.TryGetValue("Amount", out var amountBytes) ||
                !double.TryParse(System.Text.Encoding.UTF8.GetString(amountBytes), out var amount))
            {
                TempData["Message"] = "Không tìm thấy số tiền thanh toán.";
                return RedirectToAction("PaymentFail");
            }

            var hoaDon = new HoaDon // Đảm bảo đây là mô hình từ AHTB_TimBanCungGu_MVC.Models
            {
                NguoiMua = User.Identity.Name,
                GoiPremium = "Tên gói Premium",
                NgayHetHan = DateTime.Now.AddMonths(1),
                TongTien = amount,
                PhuongThucThanhToan = "VNPay",
                TrangThai = "Đã thanh toán",
                NgayThanhToan = DateTime.Now
            };

            // Lưu hóa đơn vào database...
            try
            {
                _context.HoaDon.Add(hoaDon); // Đảm bảo đây là DbSet từ DBAHTBContext cho MVC
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Lỗi khi lưu hóa đơn: {ex.Message}");
                return RedirectToAction("PaymentFail");
            }

            return RedirectToAction("PaymentSuccess");
        }
        }
}
