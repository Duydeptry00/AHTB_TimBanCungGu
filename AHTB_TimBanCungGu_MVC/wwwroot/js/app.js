document.addEventListener("DOMContentLoaded", function () {
    let items = document.querySelectorAll('.slider .list .item');
    let thumbnails = document.querySelectorAll('.thumbnail .item');

    // Cấu hình
    let countItem = items.length;
    let itemActive = 0;

    // Tự động chạy slider mỗi 5 giây
    let refreshInterval = setInterval(() => {
        itemActive = (itemActive + 1) % countItem; // Chuyển sang slide kế tiếp
        showSlider();
    }, 10000);

    function showSlider() {
        // Xóa item và thumbnail hiện đang hoạt động
        let itemActiveOld = document.querySelector('.slider .list .item.active');
        let thumbnailActiveOld = document.querySelector('.thumbnail .item.active');

        if (itemActiveOld) itemActiveOld.classList.remove('active');
        if (thumbnailActiveOld) thumbnailActiveOld.classList.remove('active');

        // Kích hoạt item và thumbnail mới
        items[itemActive].classList.add('active');
        thumbnails[itemActive].classList.add('active');
    }

    // Nhấp vào thumbnail để chuyển đến slide tương ứng
    thumbnails.forEach((thumbnail, index) => {
        thumbnail.addEventListener('click', () => {
            itemActive = index;
            showSlider();
        });
    });
});