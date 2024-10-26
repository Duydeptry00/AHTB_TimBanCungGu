document.addEventListener('DOMContentLoaded', function () {
    const sidebar = document.querySelector('.sidebar');
    const navbarToggler = document.querySelector('.navbar-toggler');

    navbarToggler.addEventListener('click', function () {
        sidebar.classList.toggle('show');
    });

    document.querySelectorAll('.nav-link').forEach(link => {
        link.addEventListener('click', function () {
            sidebar.classList.remove('show');
        });
    });
});

function HandleLogout() {
    // Add your logout logic here
    alert('Logged out successfully!');
}
