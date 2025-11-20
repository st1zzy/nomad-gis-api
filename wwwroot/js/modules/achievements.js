import { apiFetch } from './api.js';
import { paginationState, renderTable, setupSearch, openModal, closeModal } from './ui.js';

const entityType = 'achievements';
const achievementForm = document.getElementById("achievement-form");
const badgePreview = document.getElementById("achievement-badge-preview");
const badgeInput = document.getElementById("achievement-badge");

export function initAchievements() {
    setupSearch(entityType, 'achievement-search', attachEventHandlers);
    document.getElementById("show-create-achievement-modal").addEventListener("click", () => showAchievementModal("create"));
    achievementForm.addEventListener("submit", handleAchievementFormSubmit);
    // Обработчик для предпросмотра изображения
     badgeInput.addEventListener('change', event => {
        const file = event.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = e => {
                badgePreview.src = e.target.result;
                badgePreview.style.display = 'block';
            }
            reader.readAsDataURL(file);
        } else {
             badgePreview.src = '';
             badgePreview.style.display = 'none';
        }
    });
    // console.log("Achievements module initialized");
}

export async function loadAchievements() {
    const achievements = await apiFetch("/api/v1/achievements");
    paginationState[entityType].data = achievements || [];
    renderTable(entityType, attachEventHandlers);
}

export function attachEventHandlers(container) {
     container.querySelectorAll(".btn-edit-achievement").forEach(button => {
        button.addEventListener("click", (e) => {
            const achId = e.target.dataset.achievementId;
            const ach = paginationState[entityType].data.find(a => a.id === achId);
            if(ach) showAchievementModal("edit", ach);
        });
    });
    container.querySelectorAll(".btn-delete-achievement").forEach(button => {
        button.addEventListener("click", (e) => deleteAchievement(e.target.dataset.achievementId));
    });
}


function showAchievementModal(mode, achievement = null) {
    const modalTitle = document.getElementById("achievement-modal-title");
    achievementForm.reset();
    badgeInput.value = null; // Сброс файла
    badgePreview.src = "";
    badgePreview.style.display = "none";
    document.getElementById("achievement-id").value = "";
    document.getElementById("achievement-code").disabled = false;


    if (mode === "create") {
        modalTitle.textContent = "Создать достижение";
    } else {
        modalTitle.textContent = "Редактировать достижение";
        document.getElementById("achievement-id").value = achievement.id;
        document.getElementById("achievement-code").value = achievement.code;
        document.getElementById("achievement-code").disabled = true; // Код нельзя менять
        document.getElementById("achievement-title").value = achievement.title;
        document.getElementById("achievement-desc").value = achievement.description || "";
        document.getElementById("achievement-reward").value = achievement.rewardPoints;

        if (achievement.badgeImageUrl) {
            badgePreview.src = achievement.badgeImageUrl;
            badgePreview.style.display = "block";
        }
    }
    openModal("achievement-modal");
}

async function handleAchievementFormSubmit(e) {
    e.preventDefault();
    const id = document.getElementById("achievement-id").value;
    const isEdit = !!id;
    
    const formData = new FormData();
    formData.append("Title", document.getElementById("achievement-title").value);
    formData.append("Description", document.getElementById("achievement-desc").value);
    formData.append("RewardPoints", parseInt(document.getElementById("achievement-reward").value, 10));

    const badgeFile = badgeInput.files[0];
    if (badgeFile) {
        formData.append("BadgeFile", badgeFile);
    }
    
    let endpoint, method;
    if (isEdit) {
        endpoint = `/api/v1/achievements/${id}`;
        method = "PUT";
        // Code не отправляем при редактировании
    } else {
        endpoint = "/api/v1/achievements";
        method = "POST";
        formData.append("Code", document.getElementById("achievement-code").value); // Отправляем Code при создании
    }
    
    const result = await apiFetch(endpoint, { method, body: formData });
    if (result) {
        closeModal("achievement-modal");
        loadAchievements(); // Перезагружаем и рендерим
    }
}

async function deleteAchievement(achievementId) {
    if (!confirm(`Удалить достижение ${achievementId}? Значок также будет удален.`)) return;
    const result = await apiFetch(`/api/v1/achievements/${achievementId}`, { method: "DELETE" });
    if (result) {
        paginationState[entityType].data = paginationState[entityType].data.filter(a => a.id !== achievementId);
        renderTable(entityType, attachEventHandlers); // Перерисовываем
    }
}