// wwwroot/js/modules/theme.js

const themeToggleBtn = document.getElementById('theme-toggle');
const htmlElement = document.documentElement; // Получаем элемент <html>

// Функция для применения темы
function applyTheme(theme) {
    htmlElement.setAttribute('data-theme', theme);
    localStorage.setItem('adminTheme', theme); // Сохраняем выбор
    // Обновляем иконку кнопки (можно использовать более сложные иконки)
    themeToggleBtn.textContent = theme === 'dark' ? '☀️' : '🌙';
     themeToggleBtn.title = theme === 'dark' ? 'Светлая тема' : 'Темная тема';
}

// Функция для переключения темы
function toggleTheme() {
    const currentTheme = htmlElement.getAttribute('data-theme') || 'dark'; // По умолчанию темная
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
    applyTheme(newTheme);
}

// Инициализация темы при загрузке страницы
export function initTheme() {
    const savedTheme = localStorage.getItem('adminTheme');
    // Проверяем системные настройки, если нет сохраненной темы
    const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
    const initialTheme = savedTheme || (prefersDark ? 'dark' : 'light');

    applyTheme(initialTheme); // Применяем тему

    // Добавляем обработчик на кнопку
    themeToggleBtn.addEventListener('click', toggleTheme);
    console.log("Theme module initialized with theme:", initialTheme);
}