# --- Этап 1: Сборка проекта ---
# Используем официальный образ .NET 8 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем .sln и .csproj файлы
COPY nomad_gis_V2.sln .
COPY nomad_gis_V2.csproj .

# Восстанавливаем зависимости
RUN dotnet restore "nomad_gis_V2.sln"

# Копируем весь остальной код
COPY . .

# Публикуем приложение в папку /app/publish
RUN dotnet publish "nomad_gis_V2.csproj" -c Release -o /app/publish

# --- Этап 2: Запуск ---
# Используем более легкий образ ASP.NET 8
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://*:10000

# Указываем, как запускать приложение
# Docker-контейнер запустит ваш .NET-проект
ENTRYPOINT ["dotnet", "nomad_gis_V2.dll"]