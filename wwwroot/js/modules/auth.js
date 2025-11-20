// wwwroot/js/modules/auth.js
import { setToken, setRefreshToken, setUserId, setDeviceId } from './api.js'; // Импортируем сеттеры

export function initAuth() {
    // Считываем все данные из localStorage
    const storedToken = localStorage.getItem("adminToken");
    const storedRefreshToken = localStorage.getItem("adminRefreshToken");
    const storedUserId = localStorage.getItem("adminUserId");
    const storedDeviceId = localStorage.getItem("adminDeviceId");

    // Проверяем наличие всех необходимых данных
    if (!storedToken || !storedRefreshToken || !storedUserId || !storedDeviceId) {
        console.log("Authentication tokens not found, redirecting to login.");
        logout(); // Очищаем все на всякий случай
        return false;
    }

    // Устанавливаем данные для модуля api.js
    setToken(storedToken);
    setRefreshToken(storedRefreshToken);
    setUserId(storedUserId);
    setDeviceId(storedDeviceId);

    console.log("Tokens found, proceeding.");
    return true;
}

export function logout() {
    console.log("Logging out and clearing tokens.");
    // Очищаем все связанные данные
    localStorage.removeItem("adminToken");
    localStorage.removeItem("adminRefreshToken");
    localStorage.removeItem("adminUserId");
    localStorage.removeItem("adminDeviceId");

    // Сбрасываем токены в api.js
    setToken(null);
    setRefreshToken(null);
    setUserId(null);
    setDeviceId(null);

    // Перенаправляем на логин только если мы не на странице логина
    if (!window.location.pathname.endsWith('/login.html')) {
         window.location.href = "/login.html";
    }
}