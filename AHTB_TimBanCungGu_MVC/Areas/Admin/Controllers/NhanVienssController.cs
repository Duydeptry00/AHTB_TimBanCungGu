using AHTB_TimBanCungGu_API.Models;
using AHTB_TimBanCungGu_API.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AHTB_TimBanCungGu_MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class NhanVienssController : Controller
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "http://localhost:15172/api/NhanViens";

        public NhanVienssController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET: Admin/NhanVienss
        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync($"{ApiBaseUrl}");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var nhanViens = JsonConvert.DeserializeObject<List<NhanVienVM>>(jsonData);
                return View(nhanViens);
            }
            return View(new List<NhanVienVM>());
        }

        // GET: Admin/NhanVienss/Details/5
        public async Task<IActionResult> Details(string id)
        {
            var response = await _httpClient.GetAsync($"{ApiBaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var nhanVien = JsonConvert.DeserializeObject<User>(jsonData);
                return View(nhanVien);
            }
            return NotFound();
        }

        // GET: Admin/NhanVienss/Create
        public IActionResult Create()
        {
            return PartialView("_CreateUser", new NhanVienVM()); // Trả về Partial View với đối tượng User mới
        }

        // POST: Admin/NhanVienss/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NhanVienVM nhanVien)
        {
            if (ModelState.IsValid)
            {
                var jsonContent = JsonConvert.SerializeObject(nhanVien);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{ApiBaseUrl}", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                    return RedirectToAction(nameof(Index)); // Chuyển hướng về danh sách người dùng
                }
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm nhân viên.";
            }

            // Nếu không hợp lệ, trả lại Partial View với thông tin đã nhập
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(string id)
        {
            var response = await _httpClient.GetAsync($"{ApiBaseUrl}/{id}");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var nhanVien = JsonConvert.DeserializeObject<NhanVienVM>(jsonData);
                return PartialView("_EditUserPartial", nhanVien); // Trả về partial view
            }
            return NotFound();
        }

        // POST: Admin/NhanVienss/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, NhanVienVM nhanVien)
        {
            nhanVien.IdNhanVien = id;
            if (ModelState.IsValid)
            {
                var jsonContent = JsonConvert.SerializeObject(nhanVien);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{ApiBaseUrl}/{id}", content);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Sửa nhân viên thành công!";
                    return RedirectToAction(nameof(Index));
                }
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi sửa nhân viên.";
            }
            return View(nhanVien);
        }

        // POST: Admin/NhanVienss/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(NhanVienVM id)
        {
            var response = await _httpClient.DeleteAsync($"{ApiBaseUrl}/{id.IdNhanVien}");
            if (response.IsSuccessStatusCode)
            {
                return Ok(); // Trả về OK nếu xóa thành công
            }
            return Problem("Có lỗi xảy ra khi xóa nhân viên.");
        }

    }
}
