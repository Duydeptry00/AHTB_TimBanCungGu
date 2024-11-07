const notificationBtn = document.getElementById('notificationBtn');
const notificationDropdown = document.getElementById('notificationDropdown');
const accountBtn = document.getElementById('accountBtn');
const accountDropdown = document.getElementById('accountDropdown');

notificationBtn.addEventListener('click', () => {
    notificationDropdown.style.display = notificationDropdown.style.display === 'block' ? 'none' : 'block';
    accountDropdown.style.display = 'none'; // Đóng dropdown tài khoản nếu đang mở
});

accountBtn.addEventListener('click', () => {
    accountDropdown.style.display = accountDropdown.style.display === 'block' ? 'none' : 'block';
    notificationDropdown.style.display = 'none'; // Đóng dropdown thông báo nếu đang mở
});

// Đóng dropdown khi nhấp ra ngoài
document.addEventListener('click', (e) => {
    if (!e.target.closest('.dropdown-container')) {
        notificationDropdown.style.display = 'none';
        accountDropdown.style.display = 'none';
    }
});