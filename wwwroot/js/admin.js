document.addEventListener("DOMContentLoaded", () => {
    const token = localStorage.getItem("adminToken");
    if (!token) {
        window.location.href = "/login.html";
        return;
    }

    const headers = { "Authorization": `Bearer ${token}` };

    // V-- ГЛОБАЛЬНОЕ СОСТОЯНИЕ ПАГИНАЦИИ И ПОИСКА --V
    const paginationState = {
        users: { data: [], currentPage: 1, pageSize: 10, searchTerm: '' },
        points: { data: [], currentPage: 1, pageSize: 10, searchTerm: '' },
        achievements: { data: [], currentPage: 1, pageSize: 10, searchTerm: '' },
        messages: { data: [], currentPage: 1, pageSize: 10, searchTerm: '', pointId: null } 
    };
    // ^-- КОНЕЦ --^
    
    // --- API Helper ---
    async function apiFetch(endpoint, options = {}) {
        // ... (код без изменений) ...
        const defaultHeaders = { ...headers }; 
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
            if (response.status === 401 || response.status === 403) { logout(); return; }
            if (!response.ok) {
                 const errorData = await response.json();
                 alert(`Ошибка API: ${errorData.message || response.statusText}`);
                 return null;
            }
            if (response.status === 204) { return true; }
            return await response.json();
        } catch (error) {
            console.error("API Fetch Error:", error);
            alert("Сетевая ошибка или сервер недоступен.");
            return null;
        }
    }

    // --- Управление Табами ---
    const tabs = document.querySelectorAll(".tab-link");
    const tabContents = document.querySelectorAll(".tab-content");
    tabs.forEach(tab => {
        tab.addEventListener("click", () => {
             // ... (код переключения табов без изменений) ...
            const targetTab = tab.dataset.tab;
            tabs.forEach(t => t.classList.remove("active"));
            tab.classList.add("active");
            tabContents.forEach(c => c.classList.remove("active"));
            document.getElementById(`${targetTab}-tab`).classList.add("active");

            if (targetTab === "dashboard") loadDashboard();
            if (targetTab === "users") loadUsers(); // Загрузит и отобразит первую страницу
            if (targetTab === "points") loadMapPoints();
            if (targetTab === "achievements") loadAchievements();
            if (targetTab === "moderation") loadPointsForModeration(); // Загрузит точки, но не сообщения
        });
    });

    // --- Выход ---
    document.getElementById("logout-button").addEventListener("click", logout);
    function logout() { /* ... (без изменений) ... */ localStorage.removeItem("adminToken"); window.location.href = "/login.html"; }

    // --- Дашборд ---
    async function loadDashboard() { /* ... (код без изменений) ... */ 
        const stats = await apiFetch("/api/v1/dashboard/stats");
        if (!stats) { /* ... обработка ошибки ... */ return; }
        document.getElementById("stat-total-users").textContent = stats.totalUsers;
        document.getElementById("stat-new-users").textContent = `${stats.newUsersToday} новых сегодня`;
        document.getElementById("stat-total-points").textContent = stats.totalMapPoints;
        document.getElementById("stat-total-messages").textContent = stats.totalMessages;
        document.getElementById("stat-new-messages").textContent = `${stats.newMessagesToday} новых сегодня`;
        document.getElementById("stat-total-unlocks").textContent = stats.totalUnlocks;
        document.getElementById("stat-total-achievements").textContent = stats.totalAchievementsWon;
    }

    // V-- НОВАЯ ФУНКЦИЯ: РЕНДЕРИНГ ТАБЛИЦЫ С ПАГИНАЦИЕЙ --V
    function renderTable(entityType) {
        const state = paginationState[entityType];
        const container = document.getElementById(`${entityType}-table-container`);
        
        // 1. Фильтрация данных по searchTerm
        const searchTerm = state.searchTerm.toLowerCase();
        const filteredData = state.data.filter(item => {
            if (!searchTerm) return true;
            if (entityType === 'users') {
                return item.username.toLowerCase().includes(searchTerm) || item.email.toLowerCase().includes(searchTerm);
            }
            if (entityType === 'points') {
                return item.name.toLowerCase().includes(searchTerm);
            }
            if (entityType === 'achievements') {
                return item.code.toLowerCase().includes(searchTerm) || item.title.toLowerCase().includes(searchTerm);
            }
             if (entityType === 'messages') {
                return item.username.toLowerCase().includes(searchTerm) || item.content.toLowerCase().includes(searchTerm);
            }
            return true; // Для неизвестных типов не фильтруем
        });

        // 2. Расчет пагинации
        const totalItems = filteredData.length;
        const totalPages = Math.ceil(totalItems / state.pageSize);
        state.currentPage = Math.max(1, Math.min(state.currentPage, totalPages)); // Коррекция текущей страницы
        const startIndex = (state.currentPage - 1) * state.pageSize;
        const endIndex = startIndex + state.pageSize;
        const pageData = filteredData.slice(startIndex, endIndex);

        // 3. Генерация HTML таблицы (зависит от entityType)
        let tableHtml = '';
        if (pageData.length === 0) {
             tableHtml = `<p class="placeholder-text">${searchTerm ? 'Ничего не найдено по вашему запросу.' : 'Нет данных для отображения.'}</p>`;
        } else {
             // Генерируем заголовки и строки таблицы
             let headerHtml = '';
             let rowsHtml = '';
             
             if (entityType === 'users') {
                headerHtml = `<th>avatarUrl</th> <th>ID</th> <th>Username</th> <th>Email</th> <th>Role</th> <th>Действия</th>`;
                rowsHtml = pageData.map(user => `
                    <tr>
                        <td>${user.avatarUrl || ''}</td>
                        <td>${user.id}</td>
                        <td>${user.username}</td>
                        <td>${user.email}</td>
                        <td>
                            <select class="role-select" data-user-id="${user.id}">
                                <option value="User" ${user.role === 'User' ? 'selected' : ''}>User</option>
                                <option value="Admin" ${user.role === 'Admin' ? 'selected' : ''}>Admin</option>
                            </select>
                        </td>
                        <td>
                            <button class="btn-secondary btn-view-user" data-user-id="${user.id}">Инфо</button>
                            <button class="btn-danger btn-delete-user" data-user-id="${user.id}">Удалить</button>
                        </td>
                    </tr>
                `).join("");
             } else if (entityType === 'points') {
                  headerHtml = `<th>ID</th> <th>Название</th> <th>Coords (Lat, Lon)</th> <th>Радиус</th> <th>Действия</th>`;
                  rowsHtml = pageData.map(point => `
                    <tr>
                        <td>${point.id}</td>
                        <td>${point.name}</td>
                        <td>${point.latitude.toFixed(4)}, ${point.longitude.toFixed(4)}</td>
                        <td>${point.unlockRadiusMeters} м.</td>
                        <td>
                            <button class="btn-edit btn-edit-point" data-point-id="${point.id}">Редакт.</button>
                            <button class="btn-danger btn-delete-point" data-point-id="${point.id}">Удалить</button>
                        </td>
                    </tr>
                 `).join("");
             } else if (entityType === 'achievements') {
                 headerHtml = `<th>Значок</th> <th>ID</th> <th>Код</th> <th>Название</th> <th>Награда (опыт)</th> <th>Действия</th>`;
                 rowsHtml = pageData.map(ach => `
                    <tr>
                        <td><img src="${ach.badgeImageUrl || ''}" class="table-badge-icon" alt=""></td>
                        <td>${ach.id}</td>
                        <td>${ach.code}</td>
                        <td>${ach.title}</td>
                        <td>${ach.rewardPoints}</td>
                        <td>
                            <button class="btn-edit btn-edit-achievement" data-achievement-id="${ach.id}">Редакт.</button>
                            <button class="btn-danger btn-delete-achievement" data-achievement-id="${ach.id}">Удалить</button>
                        </td>
                    </tr>
                 `).join("");
             } else if (entityType === 'messages') {
                 headerHtml = `<th>Автор</th> <th>Сообщение</th> <th>Дата</th> <th>Лайки</th> <th>Действия</th>`;
                 rowsHtml = pageData.map(msg => `
                    <tr>
                        <td>${msg.username}<br><small>(${msg.userId})</small></td>
                        <td class="message-content">${msg.content}</td>
                        <td>${new Date(msg.createdAt).toLocaleString()}</td>
                        <td>${msg.likesCount}</td>
                        <td> <button class="btn-danger btn-delete-message" data-message-id="${msg.id}">Удалить</e> </td>
                    </tr>
                 `).join("");
             }

             tableHtml = `
                <table>
                    <thead><tr>${headerHtml}</tr></thead>
                    <tbody>${rowsHtml}</tbody>
                </table>
             `;
        }
        
        container.innerHTML = tableHtml;

        // 4. Настройка пагинации
        setupPagination(entityType, totalPages);
        
        // 5. Повторно навешиваем обработчики событий (т.к. таблица перерисована)
        attachTableEventHandlers(entityType);
    }
    // ^-- КОНЕЦ renderTable --^

    // V-- НОВАЯ ФУНКЦИЯ: НАСТРОЙКА ПАГИНАЦИИ --V
    function setupPagination(entityType, totalPages) {
        const paginationControls = document.getElementById(`${entityType}-pagination`);
        if (!paginationControls) return; // У дашборда нет пагинации

        const prevButton = paginationControls.querySelector('.prev-page');
        const nextButton = paginationControls.querySelector('.next-page');
        const pageInfo = paginationControls.querySelector('.page-info');
        const state = paginationState[entityType];

        pageInfo.textContent = `Страница ${state.currentPage} из ${totalPages || 1}`;
        prevButton.disabled = state.currentPage <= 1;
        nextButton.disabled = state.currentPage >= totalPages;

        // Удаляем старые обработчики, чтобы не дублировать
        prevButton.replaceWith(prevButton.cloneNode(true));
        nextButton.replaceWith(nextButton.cloneNode(true));
        
        // Добавляем новые
        paginationControls.querySelector('.prev-page').addEventListener('click', () => {
            if (state.currentPage > 1) {
                state.currentPage--;
                renderTable(entityType);
            }
        });
        paginationControls.querySelector('.next-page').addEventListener('click', () => {
             if (state.currentPage < totalPages) {
                state.currentPage++;
                renderTable(entityType);
            }
        });
    }
    // ^-- КОНЕЦ setupPagination --^

    // V-- НОВАЯ ФУНКЦИЯ: НАВЕШИВАНИЕ ОБРАБОТЧИКОВ НА ТАБЛИЦУ --V
    function attachTableEventHandlers(entityType) {
        const container = document.getElementById(`${entityType}-table-container`);

        if (entityType === 'users') {
            container.querySelectorAll(".role-select").forEach(select => {
                select.addEventListener("change", (e) => updateUserRole(e.target.dataset.userId, e.target.value));
            });
            container.querySelectorAll(".btn-delete-user").forEach(button => {
                button.addEventListener("click", (e) => deleteUser(e.target.dataset.userId));
            });
            container.querySelectorAll(".btn-view-user").forEach(button => {
                 button.addEventListener("click", (e) => showUserDetailModal(e.target.dataset.userId));
            });
        } else if (entityType === 'points') {
            container.querySelectorAll(".btn-edit-point").forEach(button => {
                button.addEventListener("click", (e) => {
                    const point = paginationState.points.data.find(p => p.id === e.target.dataset.pointId);
                    if(point) showPointModal("edit", point);
                });
            });
            container.querySelectorAll(".btn-delete-point").forEach(button => {
                button.addEventListener("click", (e) => deleteMapPoint(e.target.dataset.pointId));
            });
        } else if (entityType === 'achievements') {
             container.querySelectorAll(".btn-edit-achievement").forEach(button => {
                button.addEventListener("click", (e) => {
                    const ach = paginationState.achievements.data.find(a => a.id === e.target.dataset.achievementId);
                     if(ach) showAchievementModal("edit", ach);
                });
            });
            container.querySelectorAll(".btn-delete-achievement").forEach(button => {
                button.addEventListener("click", (e) => deleteAchievement(e.target.dataset.achievementId));
            });
        } else if (entityType === 'messages') {
             container.querySelectorAll(".btn-delete-message").forEach(button => {
                button.addEventListener("click", (e) => deleteMessage(e.target.dataset.messageId));
            });
        }
    }
    // ^-- КОНЕЦ attachTableEventHandlers --^
    
    // V-- НОВАЯ ФУНКЦИЯ: НАСТРОЙКА ПОИСКА --V
    function setupSearch(entityType, inputId) {
        const searchInput = document.getElementById(inputId);
        let debounceTimer;

        searchInput.addEventListener('input', (e) => {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                paginationState[entityType].searchTerm = e.target.value;
                paginationState[entityType].currentPage = 1; // Сброс на первую страницу при поиске
                renderTable(entityType);
            }, 300); // Задержка для предотвращения слишком частых перерисовок
        });
    }
    // ^-- КОНЕЦ setupSearch --^

    // --- Управление Пользователями ---
    async function loadUsers() {
        const users = await apiFetch("/api/v1/users");
        if (!users) {
            paginationState.users.data = []; // Очистить данные в случае ошибки
        } else {
            paginationState.users.data = users; // Сохраняем ВСЕ данные
        }
        paginationState.users.currentPage = 1; // Сброс страницы
        paginationState.users.searchTerm = document.getElementById('user-search').value; // Учитываем текущий поиск
        renderTable('users'); // Отображаем первую страницу
    }
    async function updateUserRole(userId, role) { /* ... (без изменений, но loadUsers() перерисует таблицу) ... */ 
         if (!confirm(`Изменить роль пользователя ${userId} на ${role}?`)) return;
        const result = await apiFetch(`/api/v1/users/${userId}/role`, { method: "PUT", body: { role } });
        if (result) { alert("Роль обновлена!"); loadUsers(); } // Перезагружаем все данные и рендерим
    }
    async function deleteUser(userId) { /* ... (без изменений, но loadUsers() перерисует таблицу) ... */ 
        if (!confirm(`Вы уверены, что хотите УДАЛИТЬ пользователя ${userId}? Это действие необратимо.`)) return;
        const result = await apiFetch(`/api/v1/users/${userId}`, { method: "DELETE" });
        if (result) { alert("Пользователь удален."); loadUsers(); } // Перезагружаем все данные и рендерим
    }
    const userDetailModal = document.getElementById("user-detail-modal");
    const userDetailContent = document.getElementById("user-detail-content");
    async function showUserDetailModal(userId) { /* ... (код без изменений) ... */ 
        userDetailContent.innerHTML = "<p>Загрузка...</p>";
        userDetailModal.style.display = "block";
        const user = await apiFetch(`/api/v1/users/${userId}/details`);
        if (!user) { /* ... обработка ошибки ... */ return; }
        // ... генерация HTML ...
         let pointsHtml = '<p>Пока нет открытых точек.</p>';
        if (user.unlockedPoints && user.unlockedPoints.length > 0) { /* ... генерация списка точек ... */ }
        let achievementsHtml = '<p>Пока нет достижений.</p>';
        if (user.achievements && user.achievements.length > 0) { /* ... генерация списка ачивок ... */ }
        userDetailContent.innerHTML = `...`; // Вставляем весь HTML
    }

    // --- Управление Точками (Map Points) ---
    const pointModal = document.getElementById("point-modal");
    const pointForm = document.getElementById("point-form");
    async function loadMapPoints() {
         const points = await apiFetch("/api/v1/points");
         if (!points) { paginationState.points.data = []; } 
         else { paginationState.points.data = points; }
         paginationState.points.currentPage = 1;
         paginationState.points.searchTerm = document.getElementById('point-search').value;
         renderTable('points');
    }
    function showPointModal(mode, point = null) { /* ... (код без изменений) ... */ }
    document.getElementById("show-create-point-modal").addEventListener("click", () => showPointModal("create"));
    pointForm.addEventListener("submit", async (e) => { /* ... (без изменений, но loadMapPoints() перерисует таблицу) ... */ 
        e.preventDefault();
        // ... сбор данных ...
        const result = await apiFetch(endpoint, { method, body: body });
        if (result) { pointModal.style.display = "none"; loadMapPoints(); } // Перезагружаем все данные и рендерим
    });
    async function deleteMapPoint(pointId) { /* ... (без изменений, но loadMapPoints() перерисует таблицу) ... */ 
        if (!confirm(`Удалить точку ${pointId}?`)) return;
        const result = await apiFetch(`/api/v1/points/${pointId}`, { method: "DELETE" });
        if (result) { loadMapPoints(); } // Перезагружаем все данные и рендерим
    }
    
    // --- Управление Достижениями (Achievements) ---
    const achievementModal = document.getElementById("achievement-modal");
    const achievementForm = document.getElementById("achievement-form");
    async function loadAchievements() {
        const achievements = await apiFetch("/api/v1/achievements");
        if (!achievements) { paginationState.achievements.data = []; } 
        else { paginationState.achievements.data = achievements; }
        paginationState.achievements.currentPage = 1;
        paginationState.achievements.searchTerm = document.getElementById('achievement-search').value;
        renderTable('achievements');
    }
    function showAchievementModal(mode, achievement = null) { /* ... (код без изменений) ... */ }
    document.getElementById("show-create-achievement-modal").addEventListener("click", () => showAchievementModal("create"));
    achievementForm.addEventListener("submit", async (e) => { /* ... (без изменений, использующий FormData, но loadAchievements() перерисует таблицу) ... */ 
        e.preventDefault();
        // ... сбор FormData ...
        const result = await apiFetch(endpoint, { method, body: formData }); 
        if (result) { achievementModal.style.display = "none"; loadAchievements(); } // Перезагружаем все данные и рендерим
    });
    async function deleteAchievement(achievementId) { /* ... (без изменений, но loadAchievements() перерисует таблицу) ... */ 
        if (!confirm(`Удалить достижение ${achievementId}?`)) return;
        const result = await apiFetch(`/api/v1/achievements/${achievementId}`, { method: "DELETE" });
        if (result) { loadAchievements(); } // Перезагружаем все данные и рендерим
    }

    // --- Модерация Сообщений ---
    const pointSelect = document.getElementById("point-select");
    const messagesTableContainer = document.getElementById("messages-table-container"); // Уже объявлен
    async function loadPointsForModeration() {
        // ... (код без изменений) ...
        const points = await apiFetch("/api/v1/points");
        if (!points) { /* ... обработка ошибки ... */ return; }
        pointSelect.innerHTML = `<option value="">-- Выберите точку --</option>${points.map(p => `<option value="${p.id}">${p.name} (ID: ${p.id.substring(0, 8)}...)</option>`).join("")}`;
        // Сброс сообщений при загрузке точек
        paginationState.messages.data = [];
        paginationState.messages.pointId = null;
        paginationState.messages.currentPage = 1;
        paginationState.messages.searchTerm = document.getElementById('message-search').value;
        renderTable('messages'); // Отобразит "Выберите точку..." или "Нет данных"
    }
    pointSelect.addEventListener("change", (e) => {
        const pointId = e.target.value;
        paginationState.messages.pointId = pointId; // Сохраняем выбранную точку
        if (pointId) {
            loadMessagesForPoint(pointId); // Загружаем и рендерим
        } else {
             paginationState.messages.data = [];
             paginationState.messages.currentPage = 1;
             renderTable('messages'); // Отобразит "Выберите точку..."
        }
    });
    async function loadMessagesForPoint(pointId) {
        const messages = await apiFetch(`/api/v1/messages/point/${pointId}`);
        if (!messages) { paginationState.messages.data = []; } 
        else { paginationState.messages.data = messages; }
        paginationState.messages.currentPage = 1; // Сброс на 1 страницу при выборе новой точки
        paginationState.messages.searchTerm = document.getElementById('message-search').value;
        renderTable('messages'); // Рендерим первую страницу
    }
    async function deleteMessage(messageId) {
        if (!confirm(`Удалить это сообщение? Действие необратимо.`)) return;
        const result = await apiFetch(`/api/v1/messages/admin/${messageId}`, { method: "DELETE" });
        if (result) {
            // Перезагружаем сообщения для ТЕКУЩЕЙ точки
            const currentPointId = paginationState.messages.pointId; 
            if (currentPointId) {
                loadMessagesForPoint(currentPointId); // Перезагрузит данные и вызовет renderTable
            }
        }
    }


    // --- Утилиты для Модальных окон ---
    document.querySelectorAll(".modal .close-btn").forEach(btn => { /* ... (код без изменений) ... */ });
    window.addEventListener("click", (e) => { /* ... (код без изменений) ... */ });

    // --- Инициализация ---
    loadDashboard();
    // Настраиваем поиск при загрузке страницы
    setupSearch('users', 'user-search');
    setupSearch('points', 'point-search');
    setupSearch('achievements', 'achievement-search');
    setupSearch('messages', 'message-search');
    
    // Примечание: Данные для других вкладок (users, points и т.д.) будут загружены
    // только при первом клике на соответствующую вкладку.
});