// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
// JavaScript function to toggle color mode and store preference

// Apply stored color mode on page load
document.addEventListener("DOMContentLoaded", function () {
    const toggleButton = document.querySelector('#toggle-dark-mode');
    const isDarkMode = localStorage.getItem('darkMode') === 'true';

    // Set initial mode based on localStorage
    if (isDarkMode) {
        document.body.classList.add('dark-mode');
    }

    toggleButton.addEventListener('click', () => {
        document.body.classList.toggle('dark-mode');
        const isDarkMode = document.body.classList.contains('dark-mode');
        localStorage.setItem('darkMode', isDarkMode);
    });
});

// Ensure dark mode is applied on every page load
document.addEventListener("DOMContentLoaded", function () {
    if (localStorage.getItem("darkMode") === "true") {
        document.body.classList.add("dark-mode");
    }
});
// Write your JavaScript code.
