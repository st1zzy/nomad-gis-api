document.addEventListener("DOMContentLoaded", () => {
    const loginForm = document.getElementById("login-form");
    const errorMessage = document.getElementById("error-message");

    loginForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        errorMessage.textContent = "";

        const identifier = document.getElementById("identifier").value;
        const password = document.getElementById("password").value;
        const deviceId = document.getElementById("deviceId").value; // Получаем Device ID

        try {
            const response = await fetch("/api/v1/auth/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ identifier, password, deviceId }) // Отправляем Device ID
            });

            const data = await response.json();

            if (response.ok) {
                // V-- СОХРАНЯЕМ ВСЕ НЕОБХОДИМЫЕ ДАННЫЕ --V
                localStorage.setItem("adminToken", data.accessToken);
                localStorage.setItem("adminRefreshToken", data.refreshToken); // <-- Сохраняем Refresh Token
                localStorage.setItem("adminUserId", data.user.id);          // <-- Сохраняем User ID
                localStorage.setItem("adminDeviceId", deviceId);            // <-- Сохраняем Device ID
                // ^-- КОНЕЦ --^
                window.location.href = "/index.html";
            } else {
                errorMessage.textContent = data.message || "Ошибка входа";
                // Очищаем старые токены на всякий случай
                localStorage.removeItem("adminToken");
                localStorage.removeItem("adminRefreshToken");
                localStorage.removeItem("adminUserId");
                localStorage.removeItem("adminDeviceId");
            }
        } catch (error) {
            console.error("Login error:", error);
            errorMessage.textContent = "Не удалось подключиться к серверу.";
        }
    });
});