// wwwroot/js/modules/map.js

let mainMap = null;
let mainMapMarkers = {}; // Объекты маркеров на основной карте { pointId: marker }
let highlightedMarker = null;

let modalMap = null;
let modalMarker = null;

const astanaCoords = [51.1694, 71.4491]; // Координаты Астаны для центрирования

// --- Основная карта ---

export function initMainMap(pointsData, onMarkerClickCallback) {
    const mapContainer = document.getElementById('points-map-container');
    if (!mapContainer) return;
    mapContainer.innerHTML = ''; // Очищаем индикатор загрузки

    // Удаляем старую карту, если она есть
    if (mainMap) {
        mainMap.remove();
        mainMap = null;
        mainMapMarkers = {};
        highlightedMarker = null;
    }

    // Создаем новую карту
    mainMap = L.map('points-map-container').setView(astanaCoords, 12); // Центр - Астана

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(mainMap);

    // Добавляем маркеры
    pointsData.forEach(point => addMarkerToMainMap(point, onMarkerClickCallback));

    // Центрируем карту по маркерам, если они есть
    if (pointsData.length > 0) {
        const group = new L.featureGroup(Object.values(mainMapMarkers));
        mainMap.fitBounds(group.getBounds().pad(0.1)); // pad для небольшого отступа
    }
}

function addMarkerToMainMap(point, onClickCallback) {
    if (!mainMap) return;

    const marker = L.marker([point.latitude, point.longitude]).addTo(mainMap);
    marker.bindPopup(`<b>${point.name}</b><br>ID: ${point.id.substring(0,8)}...`);
    marker.pointId = point.id; // Сохраняем ID точки в маркере

    marker.on('click', () => {
        highlightMarker(point.id);
        if (onClickCallback) {
            onClickCallback(point.id); // Передаем ID точки в колбэк (для прокрутки таблицы)
        }
    });

    mainMapMarkers[point.id] = marker;
}

export function updateMarkerOnMainMap(point) {
     if (mainMapMarkers[point.id]) {
         mainMapMarkers[point.id].setLatLng([point.latitude, point.longitude]);
         mainMapMarkers[point.id].setPopupContent(`<b>${point.name}</b><br>ID: ${point.id.substring(0,8)}...`);
     } else {
         // Если маркера нет (например, точка только что создана), добавляем его
         addMarkerToMainMap(point, null); // Колбэк не нужен при простом обновлении
     }
}

export function removeMarkerFromMainMap(pointId) {
    if (mainMapMarkers[pointId]) {
        mainMap.removeLayer(mainMapMarkers[pointId]);
        delete mainMapMarkers[pointId];
         if (highlightedMarker === mainMapMarkers[pointId]) {
            highlightedMarker = null;
         }
    }
}

// Подсветка маркера по ID
export function highlightMarker(pointId) {
    if (!mainMap) return;

    // Снимаем подсветку с предыдущего
    if (highlightedMarker) {
        L.DomUtil.removeClass(highlightedMarker._icon, 'highlighted-marker');
    }

    const markerToHighlight = mainMapMarkers[pointId];
    if (markerToHighlight) {
        L.DomUtil.addClass(markerToHighlight._icon, 'highlighted-marker');
        highlightedMarker = markerToHighlight;
        // Можно центрировать карту на маркере
        mainMap.setView(markerToHighlight.getLatLng(), mainMap.getZoom());
    } else {
         highlightedMarker = null;
    }
}

// --- Карта в модальном окне ---

export function initModalMap(latInput, lonInput) {
    const mapContainer = document.getElementById('point-modal-map');
    if (!mapContainer) return;

    // Удаляем старую карту
    if (modalMap) {
        modalMap.remove();
        modalMap = null;
        modalMarker = null;
    }

    modalMap = L.map('point-modal-map'); // Не устанавливаем view сразу

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OSM</a>'
    }).addTo(modalMap);

    // Маркер для перетаскивания
    modalMarker = L.marker([0, 0], { draggable: true }).addTo(modalMap);

    // Обновление полей формы при перетаскивании
    modalMarker.on('dragend', function(event) {
        const marker = event.target;
        const position = marker.getLatLng();
        latInput.value = position.lat.toFixed(6); // Обновляем поле Latitude
        lonInput.value = position.lng.toFixed(6); // Обновляем поле Longitude
        modalMap.panTo(position); // Центрируем карту на маркере
    });

     // Обновление маркера при изменении полей формы вручную
     latInput.addEventListener('input', updateModalMarkerPosition);
     lonInput.addEventListener('input', updateModalMarkerPosition);

     function updateModalMarkerPosition() {
         const lat = parseFloat(latInput.value);
         const lon = parseFloat(lonInput.value);
         if (!isNaN(lat) && !isNaN(lon)) {
             const newLatLng = L.latLng(lat, lon);
             modalMarker.setLatLng(newLatLng);
             modalMap.panTo(newLatLng);
         }
     }
}

// Установка позиции маркера и вида карты при открытии модалки
export function setModalMapPosition(lat, lon) {
    if (modalMap && modalMarker) {
        const initialLatLng = L.latLng(lat, lon);
        modalMarker.setLatLng(initialLatLng);
        modalMap.setView(initialLatLng, 15); // Устанавливаем вид

        // Небольшая задержка, чтобы карта успела отрисоваться перед инвалидацией
        setTimeout(() => {
            modalMap.invalidateSize(); // Важно для корректного отображения в модалке
         }, 100);
    }
}

// Центрирование карты Астаны по умолчанию, если координаты некорректны
export function resetModalMapPosition() {
    if (modalMap && modalMarker) {
        modalMarker.setLatLng(astanaCoords);
        modalMap.setView(astanaCoords, 12);
         setTimeout(() => {
            modalMap.invalidateSize();
         }, 100);
    }
}