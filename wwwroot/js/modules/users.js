import { apiFetch } from './api.js';
import { paginationState, renderTable, setupSearch, openModal, closeModal } from './ui.js';

const entityType = 'users';

export function initUsers() {
    setupSearch(entityType, 'user-search', attachEventHandlers);
    // console.log("Users module initialized");
}

export async function loadUsers() {
    const users = await apiFetch("/api/v1/users");
    paginationState[entityType].data = users || [];
    renderTable(entityType, attachEventHandlers);
}

export function attachEventHandlers(container) {
    container.querySelectorAll(".role-select").forEach(select => {
        select.addEventListener("change", (e) => updateUserRole(e.target.dataset.userId, e.target.value));
    });
    container.querySelectorAll(".btn-delete-user").forEach(button => {
        button.addEventListener("click", (e) => deleteUser(e.target.dataset.userId));
    });
    container.querySelectorAll(".btn-view-user").forEach(button => {
         button.addEventListener("click", (e) => showUserDetailModal(e.target.dataset.userId));
    });
}

async function updateUserRole(userId, role) {
    if (!confirm(`Изменить роль пользователя ${userId} на ${role}?`)) return;
    const result = await apiFetch(`/api/v1/users/${userId}/role`, { method: "PUT", body: { role } });
    if (result) {
        alert("Роль обновлена!");
        // Обновляем данные в state и перерисовываем
        const userIndex = paginationState[entityType].data.findIndex(u => u.id === userId);
        if (userIndex > -1) {
             paginationState[entityType].data[userIndex].role = role;
        }
        renderTable(entityType, attachEventHandlers); // Перерисовываем текущую страницу
    }
}

async function deleteUser(userId) {
    if (!confirm(`Вы уверены, что хотите УДАЛИТЬ пользователя ${userId}? Это действие необратимо.`)) return;
    const result = await apiFetch(`/api/v1/users/${userId}`, { method: "DELETE" });
    if (result) {
        alert("Пользователь удален.");
        // Удаляем из state и перерисовываем
         paginationState[entityType].data = paginationState[entityType].data.filter(u => u.id !== userId);
         renderTable(entityType, attachEventHandlers); // Перерисовываем
    }
}

// --- Детали пользователя ---
const userDetailContent = document.getElementById("user-detail-content");

async function showUserDetailModal(userId) {
    openModal("user-detail-modal");
    userDetailContent.innerHTML = `<progress indeterminate></progress>`; // Загрузчик Pico

    const user = await apiFetch(`/api/v1/users/${userId}/details`);
    if (!user) {
        userDetailContent.innerHTML = "<p>Ошибка загрузки данных пользователя.</p>";
        return;
    }

    // Генерация HTML (остается без изменений, только используется Pico)
    let pointsHtml = '<p>Пока нет открытых точек.</p>';
    if (user.unlockedPoints && user.unlockedPoints.length > 0) {
        pointsHtml = `<ul class="detail-list">${user.unlockedPoints.map(p => `<li><span>${p.mapPointName}</span><small class="date">${new Date(p.unlockedAt).toLocaleString()}</small></li>`).join("")}</ul>`;
    }
    let achievementsHtml = '<p>Пока нет достижений.</p>';
    if (user.achievements && user.achievements.length > 0) {
        achievementsHtml = `<ul class="detail-list">${user.achievements.map(a => `<li><span>${a.achievementTitle}</span><small class="date">${a.isCompleted ? new Date(a.completedAt).toLocaleString() : 'В процессе'}</small></li>`).join("")}</ul>`;
    }

    userDetailContent.innerHTML = `
        <div class="user-detail-header">
            <img src="${user.avatarUrl || ''}" alt="Avatar" class="user-detail-avatar">
            <div class="user-detail-info">
                <h4>${user.username}</h4>
                <p><small>${user.email}</small></p>
                <p>Роль: <strong>${user.role}</strong> | Активен: ${user.isActive ? '✅' : '❌'}</p>
                <div class="user-detail-stats grid">
                    <div class="user-detail-stat"><span>${user.level}</span><small>Уровень</small></div>
                    <div class="user-detail-stat"><span>${user.experience}</span><small>Опыт</small></div>
                </div>
            </div>
        </div>
        <div class="grid detail-grid">
            <div class="detail-list-container"><h5>Открытые точки (${user.unlockedPoints.length})</h5>${pointsHtml}</div>
            <div class="detail-list-container"><h5>Достижения (${user.achievements.length})</h5>${achievementsHtml}</div>
        </div>`;
}