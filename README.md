# **Nomad GIS API (v2)**

REST API для бэкэнда мобильного GIS-приложения "Nomad GIS".

Проект включает в себя:

* Аутентификацию на JWT (Access/Refresh токены)  
* Работу с геоданными (PostGIS и NetTopologySuite)  
* Поддержку S3-совместимого файлового хранилища (например, Cloudflare R2)  
* Систему пользователей, очков, достижений и сообщений  
* Админ-панель (через wwwroot)  
* Автоматическую документацию API через Swagger

## **Начало работы**

Этот гайд поможет вам настроить и запустить проект локально для разработки.

### **1\. Предварительные требования (Что скачивать)**

Перед тем как начать, убедитесь, что у вас установлено следующее ПО:

* [**.NET 8 SDK**](https://dotnet.microsoft.com/download/dotnet/8.0) (Проект использует net8.0)  
* [**PostgreSQL**](https://www.postgresql.org/download/) (локальный сервер или облачный, например, на Render)  
* **S3-совместимое хранилище** (например, [Cloudflare R2](https://www.cloudflare.com/developer-platform/r2/), AWS S3 или [MinIO](https://min.io/) для локального тестирования)  

### **2\. Настройка проекта**

1. **Клонируйте репозиторий:**  
```bash
   git clone https://github.com/SayatYuss/nomad-gis-api.git  
   cd nomad_gis_V2
```

2. **Настройте конфигурацию (appsettings):**  
   Скопируйте appsettings.example.json и переименуте его в appsettins.json.
   ```cs
   {  
     "Logging": {  
       "LogLevel": {  
         "Default": "Information",  
         "Microsoft.AspNetCore": "Warning"  
       }  
     },  
     "ConnectionStrings": {  
       "DefaultConnection": "Host=ВАШ_ХОСТ;Port=5432;Database=nomad_gis;Username=ВАШ_USER;Password=ВАШ_ПАРОЛЬ"  
     },  
     "Jwt": {  
       "Secret": "ВАШ_ОЧЕНЬ_СЛОЖНЫЙ_СЕКРЕТНЫЙ_КЛЮЧ_JWT_MINIMUM_32_СИМВОЛА",  
       "Issuer": "nomad_gis_V2",  
       "Audience": "nomad_gis_users",  
       "AccessTokenExpirationMinutes": "15",  
       "RefreshTokenExpirationDays": "7"  
     },  
     "AdminAccount": {  
       "Email": "admin@example.com",  
       "Username": "admin",  
       "Password": "СУПЕР_СЛОЖНЫЙ_ПАРОЛЬ_АДМИНА"  
     },  
     "R2Storage": {  
       "ServiceURL": "https://<account-id>.r2.cloudflarestorage.com",  
       "BucketName": "ВАШЕ-R2-ИМЯ-БАКЕТА",  
       "AccessKey": "ВАШ_ACCESS_KEY",  
       "SecretKey": "ВАШ_SECRET_KEY",  
       "PublicUrlBase": "https://r2.yourdomain.com"  
     }  
   }
   ```

### **3\. Настройка базы данных**

Этот проект использует **PostgreSQL** с расширением **PostGIS** для работы с геолокацией.

1. Создайте базу данных:  
   Создайте пустую базу данных PostgreSQL.  
2. ❗️ Включите расширение postgis:  
   Это обязательный шаг. Выполните следующий SQL-запрос в вашей новой базе данных (через psql, DBeaver или pgAdmin):
   ```sql  
   CREATE EXTENSION IF NOT EXISTS postgis;
   ```

3. Примените миграции (Migration):    
    Выполните в терминале в папке проекта:
    ```bash  
     # Убедитесь, что у вас установлен dotnet-ef  
     dotnet tool install --global dotnet-ef

     # Примените миграции  
     dotnet ef database update
    ```

### **4\. Запуск проекта**

1. **Восстановите зависимости:**  
    ```bash
    dotnet restore
    ```

2. **Запустите приложение:**  
    ```bash
    dotnet watch run 
    # или
    dotnet run
    ```