// wwwroot/js/modules/points.js
import { apiFetch } from './api.js';
import { paginationState, renderTable, setupSearch, openModal, closeModal } from './ui.js';
// V-- ИМПОРТ ФУНКЦИЙ КАРТЫ --V
import { initMainMap, updateMarkerOnMainMap, removeMarkerFromMainMap, highlightMarker, initModalMap, setModalMapPosition, resetModalMapPosition } from './map.js';
// ^-- КОНЕЦ --^

const entityType = 'points';
const pointForm = document.getElementById("point-form");
const latInput = document.getElementById("point-lat"); // Сохраняем ссылки на поля
const lonInput = document.getElementById("point-lon");

export function initPoints() {
    setupSearch(entityType, 'point-search', attachEventHandlers);
    document.getElementById("show-create-point-modal").addEventListener("click", () => showPointModal("create"));
    pointForm.addEventListener("submit", handlePointFormSubmit);

    // Инициализируем карту в модалке ОДИН РАЗ при загрузке модуля
    initModalMap(latInput, lonInput);

    console.log("Points module initialized");
}

export async function loadMapPoints() {
    document.getElementById('points-map-container').innerHTML = '<progress indeterminate></progress>'; // Показываем загрузку карты
    const points = await apiFetch("/api/v1/points");
    paginationState[entityType].data = points || [];

    // Инициализируем основную карту с полученными данными
    initMainMap(points || [], (pointId) => {
         // Колбэк при клике на маркер - подсвечиваем строку в таблице
         highlightTableRow(pointId);
    });

    renderTable(entityType, attachEventHandlers);
}

export function attachEventHandlers(container) {
    container.querySelectorAll(".btn-edit-point").forEach(button => {
        button.addEventListener("click", (e) => {
            const pointId = e.target.dataset.pointId;
            const point = paginationState[entityType].data.find(p => p.id === pointId);
            if(point) {
                highlightMarker(pointId); // Подсвечиваем маркер на основной карте
                showPointModal("edit", point);
            }
        });
    });
    container.querySelectorAll(".btn-delete-point").forEach(button => {
        button.addEventListener("click", (e) => deleteMapPoint(e.target.dataset.pointId));
    });

    // V-- ДОБАВЛЯЕМ КЛИК НА СТРОКУ ТАБЛИЦЫ ДЛЯ ПОДСВЕТКИ МАРКЕРА --V
    container.querySelectorAll("tbody tr").forEach(row => {
        row.addEventListener('click', (e) => {
             // Ищем кнопку редактирования в строке, чтобы получить ID
            const editButton = row.querySelector('.btn-edit-point');
            if (editButton && !e.target.closest('button')) { // Не срабатывать при клике на кнопки
                const pointId = editButton.dataset.pointId;
                highlightMarker(pointId);
                highlightTableRow(pointId);
            }
        });
    });
    // ^-- КОНЕЦ --^
}

// V-- НОВАЯ ФУНКЦИЯ ПОДСВЕТКИ СТРОКИ --V
function highlightTableRow(pointId) {
    const tableContainer = document.getElementById(`${entityType}-table-container`);
    // Снимаем подсветку со всех строк
    tableContainer.querySelectorAll("tbody tr").forEach(r => r.classList.remove('highlighted-row'));
     // Находим нужную строку (по кнопке редактирования) и подсвечиваем
    const rowToHighlight = tableContainer.querySelector(`.btn-edit-point[data-point-id="${pointId}"]`)?.closest('tr');
    if (rowToHighlight) {
        rowToHighlight.classList.add('highlighted-row');
        // Плавно прокручиваем к строке
        rowToHighlight.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }
}
// ^-- КОНЕЦ --^


function showPointModal(mode, point = null) {
    const modalTitle = document.getElementById("point-modal-title");
    pointForm.reset();
    document.getElementById("point-id").value = "";

    if (mode === "create") {
        modalTitle.textContent = "Создать точку";
        // Центрируем карту в модалке на Астане
        resetModalMapPosition();
    } else {
        modalTitle.textContent = "Редактировать точку";
        document.getElementById("point-id").value = point.id;
        document.getElementById("point-name").value = point.name;
        latInput.value = point.latitude; // Используем сохраненные ссылки
        lonInput.value = point.longitude;
        document.getElementById("point-radius").value = point.unlockRadiusMeters;
        document.getElementById("point-desc").value = point.description || "";
        // Устанавливаем позицию карты в модалке
        setModalMapPosition(point.latitude, point.longitude);
    }
    openModal("point-modal");
}

async function handlePointFormSubmit(e) {
    e.preventDefault();
    const id = document.getElementById("point-id").value;
    const isEdit = !!id;
    const body = {
        name: document.getElementById("point-name").value,
        latitude: parseFloat(latInput.value), // Используем ссылки
        longitude: parseFloat(lonInput.value),
        unlockRadiusMeters: parseFloat(document.getElementById("point-radius").value),
        description: document.getElementById("point-desc").value,
    };
    const endpoint = isEdit ? `/api/v1/points/${id}` : "/api/v1/points";
    const method = isEdit ? "PUT" : "POST";

    const result = await apiFetch(endpoint, { method, body });
    if (result) {
        closeModal("point-modal");
        // Обновляем/добавляем данные в state
        if (isEdit) {
            const index = paginationState[entityType].data.findIndex(p => p.id === id);
            if (index > -1) paginationState[entityType].data[index] = { ...paginationState[entityType].data[index], ...body, id: id }; // Обновляем
            updateMarkerOnMainMap({ ...body, id: id }); // Обновляем маркер на основной карте
        } else {
            paginationState[entityType].data.push(result); // Добавляем новую точку (API возвращает созданный объект)
            updateMarkerOnMainMap(result); // Добавляем маркер
        }
        renderTable(entityType, attachEventHandlers); // Перерисовываем таблицу
    }
}

async function deleteMapPoint(pointId) {
    if (!confirm(`Удалить точку ${pointId}?`)) return;
    const result = await apiFetch(`/api/v1/points/${pointId}`, { method: "DELETE" });
    if (result) {
        paginationState[entityType].data = paginationState[entityType].data.filter(p => p.id !== pointId);
        removeMarkerFromMainMap(pointId); // Удаляем маркер с основной карты
        renderTable(entityType, attachEventHandlers); // Перерисовываем
    }
}