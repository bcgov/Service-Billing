const navToggle = document.getElementById('nav-toggle');
const navbar = document.getElementById('navbar');

// Toggle the 'open' class on click
navToggle.addEventListener('click', function () {
    navbar.classList.toggle('open'); // Add or remove 'open' class
});
