document.addEventListener('DOMContentLoaded', () => {
    const menuToggle = document.querySelector('.menu-toggle');
    const closeBtn = document.querySelector('.close-menu');
    const nav = document.querySelector('#main-nav');
    const header = document.querySelector('header');

    if (!menuToggle || !nav) return;

    menuToggle.addEventListener('click', () => {
        nav.classList.add('active');
    });

    closeBtn.addEventListener('click', () => {
        nav.classList.remove('active');
    });

    nav.querySelectorAll('a').forEach(link => {
        link.addEventListener('click', () => {
            nav.classList.remove('active');
        });
    });

    nav.addEventListener('click', (e) => {
        if (e.target === nav) {
            nav.classList.remove('active');
        }
    });

    // Header scroll transparency effect
    window.addEventListener('scroll', () => {
        if (window.scrollY > 50) {
            header.classList.add('scrolled');
        } else {
            header.classList.remove('scrolled');
        }
    });
});

document.addEventListener('DOMContentLoaded', function () {
    const adminToggle = document.querySelector('#adminDropdown .dropdown-toggle');
    const adminMenu = document.querySelector('.custom-dropdown-menu');

    if (adminToggle && adminMenu) {
        adminToggle.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();
            adminMenu.classList.toggle('show');
        });
    }

    // Close dropdown when clicking anywhere else on the screen
    document.addEventListener('click', function (e) {
        if (adminMenu && !adminMenu.contains(e.target) && !adminToggle.contains(e.target)) {
            adminMenu.classList.remove('show');
        }
    });
});