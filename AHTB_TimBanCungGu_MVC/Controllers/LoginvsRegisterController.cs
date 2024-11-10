using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;
using AHTB_TimBanCungGu_API.ViewModels;
using System.Reflection.Metadata;

namespace AHTB_TimBanCungGu_MVC.Controllers
{
    public class LoginvsRegisterController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "http://localhost:15172/api";

        public LoginvsRegisterController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                ViewBag.Message = "Vui lòng nhập tên đăng nhập và mật khẩu.";
                return View();
            }

            try
            {
                var request = new { UserName = userName, Password = password };
                var response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}/Logins/login", request);

                if (response.IsSuccessStatusCode)
                {
                    // Giả sử API trả về token nếu đăng nhập thành công
                    var responseData = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseData);

                    // Lưu JWT token vào Session hoặc Cookie (Session trong trường hợp này)
                    HttpContext.Session.SetString("JwtToken", tokenResponse.Token);
                    HttpContext.Session.SetString("UserType", tokenResponse.UserType);
                    HttpContext.Session.SetString("TempUserName", userName);
                    ViewBag.ShowSuccessModal = true;
                    if (tokenResponse.UserType == "Admin") 
                    {
                        ViewBag.ShowInterface = "Admin";
                    }
                    else if (tokenResponse.UserType == "Nhân Viên")
                    {
                        ViewBag.ShowInterface = "Nhân Viên";
                    }
                    return View();
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    ViewBag.Message = "Tên đăng nhập hoặc mật khẩu không đúng.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Đã có lỗi xảy ra: {ex.Message}";
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string userName, string password, string email)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
            {
                ViewBag.Message = "Vui lòng nhập tên đăng nhập, mật khẩu và email.";
                return View();
            }

            if (!IsValidEmail(email))
            {
                ViewBag.Message = "Vui lòng nhập email hợp lệ.";
                return View();
            }

            // Lưu thông tin đăng ký tạm thời vào session
            HttpContext.Session.SetString("TempUserName", userName);
            HttpContext.Session.SetString("TempPassword", password);
            HttpContext.Session.SetString("TempEmail", email);

            try
            {
                var request = new { Email = email };
                string esmail = request.Email;
                var response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}/OTPs/send-otp", esmail);

                if (response.IsSuccessStatusCode)
                {
                    ViewBag.Message = "OTP đã được gửi qua email. Vui lòng kiểm tra và xác nhận.";
                    return RedirectToAction("VerifyOtp", new { email });
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    ViewBag.Message = $"Lỗi khi gửi OTP: {errorMessage}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Đã có lỗi xảy ra: {ex.Message}";
            }

            return View();
        }

        public IActionResult VerifyOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            // Lưu thời gian hiện tại vào Session khi người dùng yêu cầu OTP
            DateTime currentTime = DateTime.Now;
            // Lưu thời gian bắt đầu đếm ngược vào Session
            HttpContext.Session.SetString("otpStartTime", currentTime.ToString());

            ViewBag.Email = email;
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string email, string otp)
        {
            if (string.IsNullOrEmpty(otp))
            {
                ViewBag.Message = "Vui lòng nhập mã OTP.";
                ViewBag.Email = email;
                return View();
            }

            try
            {
                var request = new { Email = email, Otp = otp };
                var response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}/OTPs/verify-otp", request);

                if (response.IsSuccessStatusCode)
                {
                    // OTP hợp lệ, lấy thông tin đăng ký tạm thời từ Session
                    var userName = HttpContext.Session.GetString("TempUserName");
                    var password = HttpContext.Session.GetString("TempPassword");
                    var tempEmail = HttpContext.Session.GetString("TempEmail");

                    if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(tempEmail))
                    {
                        ViewBag.Message = "Không tìm thấy thông tin đăng ký tạm thời.";
                        return RedirectToAction("Register");
                    }

                    // Gửi yêu cầu đăng ký tài khoản tới API
                    var registerRequest = new { UserName = userName, Password = password, Email = tempEmail };
                    var registerResponse = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}/Logins/register", registerRequest);

                    if (registerResponse.IsSuccessStatusCode)
                    {
                        // Đăng ký thành công, xóa thông tin tạm thời
                        HttpContext.Session.Remove("TempUserName");
                        HttpContext.Session.Remove("TempPassword");
                        HttpContext.Session.Remove("TempEmail");

                        ViewBag.Message = "Đăng ký thành công! Vui lòng đăng nhập.";
                        return View();
                    }
                    else
                    {
                        var errorMessage = await registerResponse.Content.ReadAsStringAsync();
                        ViewBag.Message = $"Lỗi khi đăng ký tài khoản: {errorMessage}";
                    }
                }
                else
                {
                    ViewBag.Message = "OTP không hợp lệ hoặc đã hết hạn.";
                    ViewBag.Email = email;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = $"Đã có lỗi xảy ra: {ex.Message}";
                ViewBag.Email = email;
            }

            return View();
        }
        // Hiển thị trang quên mật khẩu
        public IActionResult QuenMatKhau()
        {

            return View();
        }

        // Phương thức xử lý quên mật khẩu
        [HttpPost]
        public async Task<IActionResult> QuenMatKhau(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập địa chỉ email.");
                return View();
            }
            string local = GlobalSettings.MvcBaseUrl;
            var content = new StringContent(
                JsonSerializer.Serialize(new { email, local }),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Account/ForgotPassword", content);

            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = "Đường link đặt lại mật khẩu đã được gửi đến email của bạn.";
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra. Vui lòng thử lại.");
            }

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> DoiMatKhau(string token, string email)
        {
            // Kiểm tra token có hợp lệ không
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Token hoặc email không hợp lệ.");
                return View(); // Trả về view hiện tại với lỗi
            }
            var content = new StringContent(
            JsonSerializer.Serialize(new { token }),
            Encoding.UTF8,
            "application/json");

            // Gửi yêu cầu kiểm tra token đến API
            var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Account/ResetPassword", content);

            if (response.IsSuccessStatusCode)
            {
                ViewData["Email"] = email;
                ViewData["Token"] = token;
                ViewBag.Message = "Xác minh thành công! Bạn có thể đặt lại mật khẩu.";
                return View(); // Chuyển đến view cho việc đặt lại mật khẩu
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra. Vui lòng thử lại.");
                ViewData["Email"] = email; // Giữ email khi có lỗi
                return View("QuenMatKhau"); // Trả về view hiện tại với lỗi
            }
        }


        [HttpPost]
        public async Task<IActionResult> DoiMatKhau(string email, string newPassword, string confirmPassword)
        {
            // Kiểm tra trường hợp thông tin không đầy đủ
            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Message = "Thông tin đổi chưa đủ!";
                ViewData["Email"] = email; // Giữ email khi có lỗi
                return View();
            }

            // Kiểm tra nếu mật khẩu và xác nhận mật khẩu không khớp
            if (newPassword != confirmPassword)
            {

                ViewBag.Message = "Mật khẩu không khớp!";
                ViewData["Email"] = email; // Giữ email khi có lỗi
                return View();
            }

            // Chuẩn bị dữ liệu cho API
            var content = new StringContent(
                JsonSerializer.Serialize(new { email, newPassword }),
                Encoding.UTF8,
                "application/json");

            // Gửi yêu cầu đến API để đổi mật khẩu
            var response = await _httpClient.PostAsync($"{ApiBaseUrl}/Account/ChangePassword", content);

            // Kiểm tra kết quả trả về từ API
            if (response.IsSuccessStatusCode)
            {

                // Nếu thành công, hiển thị thông báo thành công và chuyển hướng đến trang đăng nhập
                ViewBag.Message = "Mật khẩu đã được đổi thành công.";
                return View();
            }
            else
            {
                // Nếu có lỗi, hiển thị thông báo lỗi
                ViewBag.Message = "Có lỗi xảy ra khi đổi mật khẩu!";
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra. Vui lòng thử lại.");
                ViewData["Email"] = email; // Giữ email khi có lỗi
                return View();
            }
        }



        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}