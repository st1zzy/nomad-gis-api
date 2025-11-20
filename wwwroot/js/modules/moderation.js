import { apiFetch } from './api.js';
import { paginationState, renderTable, setupSearch } from './ui.js';

const entityType = 'messages';
const pointSelect = document.getElementById("point-select");

export function initModeration() {
    setupSearch(entityType, 'message-search', attachEventHandlers);
    pointSelect.addEventListener("change", handlePointSelectChange);
    console.log("Moderation module initialized");
}

export async function loadPointsForModeration() {
    // Загружаем список точек для выпадающего меню
    const points = await apiFetch("/api/v1/points");
    if (!points) {
        pointSelect.innerHTML = "<option value=''>Ошибка загрузки точек</option>";
        return;
    }
    pointSelect.innerHTML = `<option value="">-- Выберите точку --</option>${points.map(p => `<option value="${p.id}">${p.name} (ID: ${p.id.substring(0, 8)}...)</option>`).join("")}`;
    
    // Сбрасываем состояние сообщений
    paginationState[entityType].data = [];
    paginationState[entityType].pointId = null;
    paginationState[entityType].currentPage = 1;
    // searchTerm уже будет взят из input при вызове renderTable
    renderTable(entityType, attachEventHandlers); // Отобразит "Выберите точку..."
}

function handlePointSelectChange(e) {
    const pointId = e.target.value;
    paginationState[entityType].pointId = pointId; // Сохраняем ID точки
    if (pointId) {
        loadMessagesForPoint(pointId); // Загружаем сообщения для выбранной точки
    } else {
        paginationState[entityType].data = [];
        paginationState[entityType].currentPage = 1;
        renderTable(entityType, attachEventHandlers); // Отобразит "Выберите точку..."
    }
}

export async function loadMessagesForPoint(pointId) {
    // Загружаем сообщения для конкретной точки
    const messages = await apiFetch(`/api/v1/messages/point/${pointId}`);
    paginationState[entityType].data = messages || [];
    paginationState[entityType].currentPage = 1; // Всегда сбрасываем на 1 при загрузке новых
    // searchTerm уже будет взят из input при вызове renderTable
    renderTable(entityType, attachEventHandlers);
}

export function attachEventHandlers(container) {
     container.querySelectorAll(".btn-delete-message").forEach(button => {
        button.addEventListener("click", (e) => deleteMessage(e.target.dataset.messageId));
    });
}

async function deleteMessage(messageId) {
    if (!confirm(`Удалить это сообщение? Действие необратимо.`)) return;
    const result = await apiFetch(`/api/v1/messages/admin/${messageId}`, { method: "DELETE" });
    if (result) {
        // Удаляем из state и перерисовываем
        paginationState[entityType].data = paginationState[entityType].data.filter(m => m.id !== messageId);
        renderTable(entityType, attachEventHandlers); // Перерисовываем
    }
}