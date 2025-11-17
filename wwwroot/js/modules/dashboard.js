import { apiFetch } from './api.js';

const elements = {
    totalUsers: document.getElementById("stat-total-users"),
    newUsers: document.getElementById("stat-new-users"),
    totalPoints: document.getElementById("stat-total-points"),
    totalMessages: document.getElementById("stat-total-messages"),
    newMessages: document.getElementById("stat-new-messages"),
    totalUnlocks: document.getElementById("stat-total-unlocks"),
    totalAchievements: document.getElementById("stat-total-achievements"),
};

export function initDashboard() {
    // Начальная инициализация, если нужна
    // console.log("Dashboard module initialized");
}

export async function loadDashboard() {
    Object.values(elements).forEach(el => el ? el.textContent = '...' : null); // Показываем загрузку

    const stats = await apiFetch("/api/v1/dashboard/stats");

    if (stats) {
        elements.totalUsers.textContent = stats.totalUsers;
        elements.newUsers.textContent = `${stats.newUsersToday} новых сегодня`;
        elements.totalPoints.textContent = stats.totalMapPoints;
        elements.totalMessages.textContent = stats.totalMessages;
        elements.newMessages.textContent = `${stats.newMessagesToday} новых сегодня`;
        elements.totalUnlocks.textContent = stats.totalUnlocks;
        elements.totalAchievements.textContent = stats.totalAchievementsWon;
    } else {
         Object.values(elements).forEach(el => el ? el.textContent = 'Ошибка' : null);
    }
}