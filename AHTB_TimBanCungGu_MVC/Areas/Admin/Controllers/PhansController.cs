using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using AHTB_TimBanCungGu_API.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AHTB_TimBanCungGu_MVC.Controllers
{
    [Area("Admin")]
    public class PhansController : Controller
    {
        private readonly HttpClient _httpClient;

        public PhansController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            string apiUrl = "http://localhost:15172/api/Phans";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var phanList = JsonConvert.DeserializeObject<IEnumerable<PhanVM>>(data);
                return View(phanList);
            }
            else
            {
                return View("Error");
            }
        }

        // GET: Phans/Create
        public async Task<IActionResult> Create()
        {
            await PopulatePhimDropDownList(); // Gọi hàm để đổ dữ liệu vào ViewBag
            return View();
        }

       
        // POST: Phans/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhanVM phanVM)
        {
            string apiUrl = "http://localhost:43947/api/Phans";
            var jsonData = JsonConvert.SerializeObject(phanVM);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                // Nếu tạo phần phim thành công, tiếp tục tạo các tập phim
                var phanCreated = JsonConvert.DeserializeObject<PhanVM>(await response.Content.ReadAsStringAsync());

                // Gọi API để tạo nhiều tập phim
                string createTapsUrl = $"http://localhost:43947/api/Taps/CreateMultiple";
                var createTapsData = new
                {
                    phanID = phanCreated.IDPhan, // Gán ID phần phim mới tạo
                    soLuongTap = phanVM.SoLuongTap // Gán số lượng tập
                };

                var tapsJsonData = JsonConvert.SerializeObject(createTapsData);
                var tapsContent = new StringContent(tapsJsonData, Encoding.UTF8, "application/json");

                var createTapsResponse = await _httpClient.PostAsync(createTapsUrl, tapsContent);
                var createTapsResponseContent = await createTapsResponse.Content.ReadAsStringAsync();

                return RedirectToAction(nameof(Index));
            }
            else
            {
                await PopulatePhimDropDownList(); // Đổ dữ liệu lại nếu có lỗi
                return View(phanVM);
            }
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string apiUrl = $"http://localhost:43947/api/Phans/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var phan = JsonConvert.DeserializeObject<PhanVM>(data);

                if (phan == null)
                {
                    return NotFound();
                }

                return View(phan);
            }
            else
            {
                return View("Error");
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string apiUrl = $"http://localhost:43947/api/Phans/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var phan = JsonConvert.DeserializeObject<PhanVM>(data);

                // Kiểm tra dữ liệu trả về từ API
                if (phan == null)
                {
                    // Nếu phan là null, trả về trang lỗi hoặc thông báo lỗi
                    return View("Error");
                }

                await PopulatePhimDropDownList(phan.PhimID);
                return View(phan);
            }
            else
            {
                return View("Error");
            }
        }

        // POST: Phans/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, PhanVM phanVM)
        {
            if (id != phanVM.IDPhan)
            {
                return BadRequest();
            }

            string apiUrl = $"http://localhost:43947/api/Phans/{id}";
            var jsonData = JsonConvert.SerializeObject(phanVM);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            // Gọi API để cập nhật thông tin phần phim
            HttpResponseMessage response = await _httpClient.PutAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                // Gọi API CountTapsByPhan để lấy số lượng tập hiện tại của phần phim
                string countTapsApiUrl = $"http://localhost:43947/api/Taps/CountByPhan/{phanVM.IDPhan}";
                HttpResponseMessage countResponse = await _httpClient.GetAsync(countTapsApiUrl);

                if (countResponse.IsSuccessStatusCode)
                {
                    var countData = await countResponse.Content.ReadAsStringAsync();
                    int currentTapCount = int.Parse(countData); // Số lượng tập hiện tại
                    int newTapCount = phanVM.SoLuongTap; // Số lượng tập mới

                    if (newTapCount > currentTapCount)
                    {
                        // Thêm các tập mới nếu số lượng yêu cầu lớn hơn số lượng hiện tại
                        var createTapsData = new
                        {
                            phanID = phanVM.IDPhan,
                            soLuongTap = newTapCount - currentTapCount // Chỉ thêm số tập còn thiếu
                        };

                        var tapsJsonData = JsonConvert.SerializeObject(createTapsData);
                        var tapsContent = new StringContent(tapsJsonData, Encoding.UTF8, "application/json");

                        // Gọi API để tạo thêm các tập
                        string createTapsUrl = $"http://localhost:43947/api/Taps/CreateMultiple";
                        await _httpClient.PostAsync(createTapsUrl, tapsContent);
                    }
                    else if (newTapCount < currentTapCount)
                    {
                        // Xóa các tập dư thừa nếu số lượng mới nhỏ hơn số lượng hiện tại
                        var deleteTapsData = new
                        {
                            phanID = phanVM.IDPhan,
                            soLuongTap = newTapCount // Chỉ giữ lại số tập cần thiết
                        };

                        var deleteTapsJsonData = JsonConvert.SerializeObject(deleteTapsData);
                        var deleteTapsContent = new StringContent(deleteTapsJsonData, Encoding.UTF8, "application/json");

                        // Gọi API để xóa các tập dư thừa
                        string deleteTapsUrl = $"http://localhost:43947/api/Taps/DeleteExcessTaps";
                        await _httpClient.PostAsync(deleteTapsUrl, deleteTapsContent);
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Gọi lại để đổ dữ liệu phim nếu có lỗi
                await PopulatePhimDropDownList(phanVM.PhimID);
                return View(phanVM);
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string apiUrl = $"http://localhost:43947/api/Phans/{id}";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var phan = JsonConvert.DeserializeObject<PhanVM>(data);

                if (phan == null)
                {
                    return NotFound();
                }

                return View(phan);
            }
            else
            {
                return View("Error");
            }
        }

        // POST: Phans/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            string apiUrl = $"http://localhost:43947/api/Phans/{id}";
            HttpResponseMessage response = await _httpClient.DeleteAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View("Error");
            }
        }

        private async Task PopulatePhimDropDownList(object selectedPhim = null)
        {
            string apiUrl = "http://localhost:43947/api/Phims";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var phimList = JsonConvert.DeserializeObject<IEnumerable<PhimVM>>(data);

                // Kiểm tra dữ liệu phimList
                if (phimList == null)
                {
                    ViewBag.PhimID = new SelectList(new List<PhimVM>());
                    return;
                }

                ViewBag.PhimID = new SelectList(phimList, "IDPhim", "TenPhim", selectedPhim);
            }
            else
            {
                ViewBag.PhimID = new SelectList(new List<PhimVM>());
            }
        }

    }
}
