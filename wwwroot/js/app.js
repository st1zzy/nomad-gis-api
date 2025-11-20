import { initAuth, logout } from './modules/auth.js';
import { initTabs, initModals } from './modules/ui.js'; // Добавили initModals
import { loadDashboard, initDashboard } from './modules/dashboard.js';
import { initUsers } from './modules/users.js';
import { initPoints } from './modules/points.js';
import { initAchievements } from './modules/achievements.js';
import { initModeration } from './modules/moderation.js';
import { initTheme } from './modules/theme.js'; // <-- ИМПОРТ ФУНКЦИИ ТЕМЫ

document.addEventListener("DOMContentLoaded", () => {
    // 0. Инициализация темы ДО проверки авторизации
    initTheme(); // <-- ВЫЗОВ ИНИЦИАЛИЗАЦИИ ТЕМЫ

    // 1. Проверка авторизации
    if (!initAuth()) {
        return;
    }

    // 2. Инициализация UI (вкладки, модальные окна)
    initTabs();
    initModals();
    // 3. Инициализация логики для каждой вкладки
    initDashboard();
    initUsers();
    initPoints();
    initAchievements();
    initModeration();

    // 4. Загрузка данных для первой активной вкладки (Дашборд)
    // loadDashboard(); // Больше не нужно, initTabs вызовет загрузку при активации

    // 5. Кнопка выхода
    document.getElementById("logout-button").addEventListener("click", logout);
});