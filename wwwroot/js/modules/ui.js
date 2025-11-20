// Глобальное состояние пагинации вынесено сюда
export const paginationState = {
    users: { data: [], currentPage: 1, pageSize: 10, searchTerm: '' },
    points: { data: [], currentPage: 1, pageSize: 10, searchTerm: '' },
    achievements: { data: [], currentPage: 1, pageSize: 10, searchTerm: '' },
    messages: { data: [], currentPage: 1, pageSize: 10, searchTerm: '', pointId: null }
};

// --- Управление Вкладками ---
export function initTabs() {
    const tabs = document.querySelectorAll("#tabs-nav .tab-link");
    const tabContents = document.querySelectorAll("main > .tab-content");

    // Функция для переключения вкладок
    const switchTab = (targetTabId) => {
        tabs.forEach(t => {
            if (t.dataset.tab === targetTabId) {
                t.classList.add('active'); // Pico использует [aria-current] или можно добавить свой стиль
                t.setAttribute('aria-current', 'page');
            } else {
                t.classList.remove('active');
                t.removeAttribute('aria-current');
            }
        });
        tabContents.forEach(c => {
            c.hidden = (c.id !== `${targetTabId}-tab`);
        });

        // Динамическая загрузка данных при переключении
        // Импорты должны быть наверху файла, поэтому используем switch
        switch (targetTabId) {
            case 'dashboard':
                import('./dashboard.js').then(module => module.loadDashboard());
                break;
            case 'users':
                import('./users.js').then(module => module.loadUsers());
                break;
            case 'points':
                 import('./points.js').then(module => module.loadMapPoints());
                 break;
            case 'achievements':
                 import('./achievements.js').then(module => module.loadAchievements());
                 break;
            case 'moderation':
                 import('./moderation.js').then(module => module.loadPointsForModeration());
                 break;
        }
    };

    tabs.forEach(tab => {
        tab.addEventListener("click", (e) => {
            e.preventDefault();
            switchTab(tab.dataset.tab);
        });
    });

    // Активируем первую вкладку (Дашборд) при загрузке
    const initialTab = 'dashboard';
    switchTab(initialTab);
}

// --- Управление Модальными Окнами (Pico.css использует <dialog>) ---
export function initModals() {
    const triggers = document.querySelectorAll("[data-target]");
    const closeButtons = document.querySelectorAll(".close");
    const dialogs = document.querySelectorAll("dialog");

    // Открыть модальное окно
    triggers.forEach(trigger => {
        trigger.addEventListener("click", event => {
            event.preventDefault();
            const modalId = trigger.dataset.target;
            const modal = document.getElementById(modalId);
            if (modal) {
                modal.showModal();
            }
        });
    });

    // Закрыть модальное окно по кнопке
    closeButtons.forEach(button => {
        button.addEventListener("click", event => {
            event.preventDefault();
            const modalId = button.dataset.target; // Используем data-target с кнопки закрытия
             const modal = document.getElementById(modalId);
             if(modal) {
                 modal.close();
             }
        });
    });

     // Закрыть модальное окно по клику вне article
    dialogs.forEach(dialog => {
        dialog.addEventListener('click', event => {
            if (event.target === dialog) {
                 dialog.close();
            }
        })
    });
}

// --- Рендеринг Таблиц и Пагинации ---
export function renderTable(entityType, attachEventHandlersCallback) {
    const state = paginationState[entityType];
    const container = document.getElementById(`${entityType}-table-container`);
    const searchInput = document.getElementById(`${entityType}-search`);
    state.searchTerm = searchInput ? searchInput.value.toLowerCase() : '';

    // Фильтрация
    const filteredData = state.data.filter(item => {
        if (!state.searchTerm) return true;
        // Логика фильтрации для каждого типа сущности
         if (entityType === 'users') return item.username.toLowerCase().includes(state.searchTerm) || item.email.toLowerCase().includes(state.searchTerm);
         if (entityType === 'points') return item.name.toLowerCase().includes(state.searchTerm);
         if (entityType === 'achievements') return item.code.toLowerCase().includes(state.searchTerm) || item.title.toLowerCase().includes(state.searchTerm);
         if (entityType === 'messages') return item.username.toLowerCase().includes(state.searchTerm) || item.content.toLowerCase().includes(state.searchTerm);
        return true;
    });

    // Пагинация
    const totalItems = filteredData.length;
    const totalPages = Math.ceil(totalItems / state.pageSize) || 1; // Минимум 1 страница
    state.currentPage = Math.max(1, Math.min(state.currentPage, totalPages));
    const startIndex = (state.currentPage - 1) * state.pageSize;
    const pageData = filteredData.slice(startIndex, startIndex + state.pageSize);

    // Генерация HTML
    container.innerHTML = generateTableHTML(entityType, pageData, state.searchTerm);

    // Настройка пагинации
    setupPagination(entityType, totalPages);

    // Навешивание обработчиков (передается как колбэк)
    if (attachEventHandlersCallback) {
        attachEventHandlersCallback(container);
    }
}

function generateTableHTML(entityType, pageData, searchTerm) {
    if (pageData.length === 0) {
        return `<p>${searchTerm ? 'Ничего не найдено.' : (entityType === 'messages' && !paginationState.messages.pointId ? 'Выберите точку...' : 'Нет данных.')}</p>`;
    }

    let headerHtml = '';
    let rowsHtml = '';

    // Генерация заголовков и строк для каждого типа
     if (entityType === 'users') {
        headerHtml = `<th>Аватар</th> <th>ID</th> <th>Username</th> <th>Email</th> <th>Роль</th> <th>Действия</th>`;
        rowsHtml = pageData.map(user => `
            <tr>
                <td><img src="${user.avatarUrl || ''}" class="table-badge-icon" alt=""></td>
                <td><small>${user.id}</small></td>
                <td>${user.username}</td>
                <td>${user.email}</td>
                <td>
                    <select class="role-select" data-user-id="${user.id}" ${user.id === 'current_admin_id' ? 'disabled' : ''}>
                        <option value="User" ${user.role === 'User' ? 'selected' : ''}>User</option>
                        <option value="Admin" ${user.role === 'Admin' ? 'selected' : ''}>Admin</option>
                    </select>
                </td>
                <td>
                    <button class="secondary outline btn-view-user" data-user-id="${user.id}">Инфо</button>
                    <button class="contrast outline btn-delete-user" data-user-id="${user.id}" ${user.id === 'current_admin_id' ? 'disabled' : ''}>Удалить</button>
                </td>
            </tr>`).join("");
    } else if (entityType === 'points') {
        headerHtml = `<th>ID</th> <th>Название</th> <th>Координаты</th> <th>Радиус</th> <th>Действия</th>`;
        rowsHtml = pageData.map(point => `
            <tr>
                <td><small>${point.id}</small></td>
                <td>${point.name}</td>
                <td><small>${point.latitude.toFixed(4)}, ${point.longitude.toFixed(4)}</small></td>
                <td>${point.unlockRadiusMeters} м.</td>
                <td>
                    <button class="secondary outline btn-edit-point" data-point-id="${point.id}">Редакт.</button>
                    <button class="contrast outline btn-delete-point" data-point-id="${point.id}">Удалить</button>
                </td>
            </tr>`).join("");
    } else if (entityType === 'achievements') {
         headerHtml = `<th>Значок</th> <th>ID</th> <th>Код</th> <th>Название</th> <th>Награда</th> <th>Действия</th>`;
         rowsHtml = pageData.map(ach => `
            <tr>
                <td><img src="${ach.badgeImageUrl || ''}" class="table-badge-icon" alt=""></td>
                <td><small>${ach.id}</small></td>
                <td><code>${ach.code}</code></td>
                <td>${ach.title}</td>
                <td>${ach.rewardPoints} XP</td>
                <td>
                    <button class="secondary outline btn-edit-achievement" data-achievement-id="${ach.id}">Редакт.</button>
                    <button class="contrast outline btn-delete-achievement" data-achievement-id="${ach.id}">Удалить</button>
                </td>
            </tr>`).join("");
    } else if (entityType === 'messages') {
        headerHtml = `<th>Автор</th> <th>Сообщение</th> <th>Дата</th> <th>Лайки</th> <th>Действия</th>`;
        rowsHtml = pageData.map(msg => `
            <tr>
                <td>${msg.username}<br><small>(${msg.userId})</small></td>
                <td class="message-content">${msg.content}</td>
                <td><small>${new Date(msg.createdAt).toLocaleString()}</small></td>
                <td>${msg.likesCount}</td>
                <td> <button class="contrast outline btn-delete-message" data-message-id="${msg.id}">Удалить</button> </td>
            </tr>`).join("");
    }


    return `
        <table role="grid">
            <thead><tr>${headerHtml}</tr></thead>
            <tbody>${rowsHtml}</tbody>
        </table>
    `;
}

// Настройка кнопок пагинации
function setupPagination(entityType, totalPages) {
    const paginationControls = document.getElementById(`${entityType}-pagination`);
    if (!paginationControls) return;
    const state = paginationState[entityType];

    const prevButtonHtml = `<button class="secondary outline prev-page" ${state.currentPage <= 1 ? 'disabled' : ''}>Назад</button>`;
    const nextButtonHtml = `<button class="secondary outline next-page" ${state.currentPage >= totalPages ? 'disabled' : ''}>Вперед</button>`;
    const pageInfoHtml = `<span class="page-info">Страница ${state.currentPage} из ${totalPages}</span>`;

    paginationControls.innerHTML = `${prevButtonHtml} ${pageInfoHtml} ${nextButtonHtml}`;

    const prevButton = paginationControls.querySelector('.prev-page');
    const nextButton = paginationControls.querySelector('.next-page');

    if (prevButton) {
        prevButton.addEventListener('click', () => {
            if (state.currentPage > 1) {
                state.currentPage--;
                // Динамический импорт и вызов рендера
                 import('./' + entityType + '.js').then(module => renderTable(entityType, module.attachEventHandlers));
            }
        });
    }
    if (nextButton) {
        nextButton.addEventListener('click', () => {
             if (state.currentPage < totalPages) {
                state.currentPage++;
                 import('./' + entityType + '.js').then(module => renderTable(entityType, module.attachEventHandlers));
            }
        });
    }
}

// --- Настройка Поиска ---
export function setupSearch(entityType, inputId, attachEventHandlersCallback) {
    const searchInput = document.getElementById(inputId);
    if (!searchInput) return;
    let debounceTimer;

    searchInput.addEventListener('input', (e) => {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
            paginationState[entityType].searchTerm = e.target.value;
            paginationState[entityType].currentPage = 1;
            renderTable(entityType, attachEventHandlersCallback);
        }, 300);
    });
}

// --- Утилиты для Модальных окон ---
// Открытие модального окна (Pico)
export function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.showModal();
    }
}
// Закрытие модального окна (Pico)
export function closeModal(modalId) {
     const modal = document.getElementById(modalId);
    if (modal) {
        modal.close();
    }
}