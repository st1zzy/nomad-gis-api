// wwwroot/js/modules/api.js
import { logout } from './auth.js';

// Переменные для хранения токенов и ID
let accessToken = null;
let refreshToken = null;
let userId = null;
let deviceId = null; // Добавили deviceId

// Флаг, чтобы избежать бесконечных попыток обновления
let isRefreshing = false;

// Сеттеры для установки значений из auth.js
export function setToken(newToken) { accessToken = newToken; }
export function setRefreshToken(newRefreshToken) { refreshToken = newRefreshToken; }
export function setUserId(newUserId) { userId = newUserId; }
export function setDeviceId(newDeviceId) { deviceId = newDeviceId; } // Сеттер для deviceId

// Геттеры (если понадобятся где-то еще)
export function getToken() { return accessToken; }
export function getRefreshToken() { return refreshToken; }
export function getUserId() { return userId; }
export function getDeviceId() { return deviceId; } // Геттер для deviceId

// Функция для попытки обновления токена
async function attemptTokenRefresh() {
    // Если уже идет попытка обновления, выходим
    if (isRefreshing) {
        console.log("Token refresh already in progress.");
        return false;
    }
    isRefreshing = true;
    console.log("Attempting token refresh...");

    if (!refreshToken || !userId || !deviceId) {
        console.error("Refresh token, User ID, or Device ID is missing for refresh attempt.");
        isRefreshing = false;
        logout(); // Недостаточно данных для обновления, выходим
        return false;
    }

    try {
        // Вызываем эндпоинт /api/v1/auth/refresh
        const response = await fetch('/api/v1/auth/refresh', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ refreshToken: refreshToken, userId: userId, deviceId: deviceId }) // Отправляем нужные данные
        });

        if (response.ok) {
            const data = await response.json();
            // Обновляем токены в localStorage и в переменных модуля
            localStorage.setItem('adminToken', data.accessToken);
            localStorage.setItem('adminRefreshToken', data.refreshToken); // Обновляем и refresh token
            setToken(data.accessToken);
            setRefreshToken(data.refreshToken);
            console.log("Token refreshed successfully.");
            isRefreshing = false;
            return true; // Обновление успешно
        } else {
            // Если обновление не удалось (например, refresh token истек)
            console.error("Token refresh failed:", response.status, await response.text());
            isRefreshing = false;
            logout(); // Выходим из системы
            return false;
        }
    } catch (error) {
        console.error("Error during token refresh:", error);
        isRefreshing = false;
        // Возможно, проблема с сетью, не обязательно выходить сразу
        // logout();
        return false;
    }
}

// Обновленный API Helper с логикой перехвата 401 и повторной попытки
export async function apiFetch(endpoint, options = {}, isRetry = false) { // Добавлен флаг isRetry
    if (!accessToken) {
        console.error("API token is not set.");
        logout();
        return null;
    }

    const currentAccessToken = accessToken; // Сохраняем токен, который будем использовать
    const defaultHeaders = { "Authorization": `Bearer ${currentAccessToken}` };
    let body = options.body;

    if (body && !(body instanceof FormData)) {
        body = JSON.stringify(body);
        defaultHeaders['Content-Type'] = 'application/json';
    } else {
        delete defaultHeaders['Content-Type'];
    }

    options.headers = { ...defaultHeaders, ...options.headers };
    options.body = body;

    try {
        const response = await fetch(endpoint, options);

        // Перехватываем 401 Unauthorized
        if (response.status === 401 && !isRetry) { // Только если это не повторная попытка
            console.warn("API returned 401, attempting token refresh...");
            const refreshSuccess = await attemptTokenRefresh();

            if (refreshSuccess) {
                console.log("Retrying original request with new token...");
                // Повторяем оригинальный запрос с новым токеном
                // Важно передать ОРИГИНАЛЬНЫЕ options (без измененных headers/body)
                // и установить флаг isRetry = true
                return await apiFetch(endpoint, { ...options, body: options.originalBody || options.body }, true);
                // Примечание: Для FormData тело не копируется легко, но так как мы его не меняли
                // в `apiFetch`, можно передать оригинальные options.
                // Если бы мы модифицировали FormData, потребовалось бы его клонировать.
            } else {
                // Если обновление не удалось, logout() уже был вызван внутри attemptTokenRefresh
                return null; // Прерываем выполнение
            }
        }

        // Обработка остальных статусов (как было раньше)
        if (response.status === 403) { // Forbidden - прав недостаточно, выходим
             console.error("API returned 403 Forbidden.");
             alert("У вас недостаточно прав для этого действия.");
             logout(); // Можно просто выйти или показать сообщение
             return null;
        }

        if (!response.ok) {
            const errorData = await response.json().catch(() => ({ message: response.statusText }));
            console.error(`API Error ${response.status}: ${errorData.message || response.statusText}`, errorData);
            alert(`Ошибка API: ${errorData.message || response.statusText}`);
            return null;
        }

        if (response.status === 204) { return true; }
        return await response.json();

    } catch (error) {
        console.error("API Fetch Network Error:", error);
        alert("Сетевая ошибка или сервер недоступен.");
        return null;
    }
}