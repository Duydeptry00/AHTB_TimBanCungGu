
    document.addEventListener("DOMContentLoaded", function () {
        let items = document.querySelectorAll('.slider .list .item');
    let next = document.getElementById('next');
    let prev = document.getElementById('prev');
    let thumbnails = document.querySelectorAll('.thumbnail .item');

    // Tham số cấu hình
    let countItem = items.length;
    let itemActive = 0;

    // Sự kiện: nhấn nút tiếp theo
    next.onclick = function () {
        itemActive = (itemActive + 1) % countItem; // Quay lại đầu danh sách
    showSlider();
        };

    // Sự kiện: nhấn nút trước
    prev.onclick = function () {
        itemActive = (itemActive - 1 + countItem) % countItem; // Quay lại cuối danh sách
    showSlider();
        };

        // Tự động chạy slider
        let refreshInterval = setInterval(() => {
        next.click();
        }, 5000);

    function showSlider() {
        // Xóa item cũ đang hoạt động
        let itemActiveOld = document.querySelector('.slider .list .item.active');
    let thumbnailActiveOld = document.querySelector('.thumbnail .item.active');

    if (itemActiveOld) itemActiveOld.classList.remove('active');
    if (thumbnailActiveOld) thumbnailActiveOld.classList.remove('active');

    // Kích hoạt item mới
    items[itemActive].classList.add('active');
    thumbnails[itemActive].classList.add('active');

    // Xóa interval tự động và khởi động lại
    clearInterval(refreshInterval);
            refreshInterval = setInterval(() => {
        next.click();
            }, 5000);
        }

        // Nhấp vào thumbnail để kích hoạt phim tương ứng
        thumbnails.forEach((thumbnail, index) => {
        thumbnail.addEventListener('click', () => {
            itemActive = index;
            showSlider();
        });
        });
    });

