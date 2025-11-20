using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using nomad_gis_V2.Models;

namespace nomad_gis_V2.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAdminUser(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            // Получаем нужные сервисы
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher<User>>();
            
            // 1. Получаем данные админа из appsettings.json
            var adminEmail = configuration["AdminAccount:Email"];
            var adminUsername = configuration["AdminAccount:Username"];
            var adminPassword = configuration["AdminAccount:Password"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword) || string.IsNullOrEmpty(adminUsername))
            {
                Console.WriteLine("Admin account credentials not found in configuration. Skipping admin seed.");
                return;
            }

            // 2. Проверяем, существует ли уже админ
            if (await context.Users.AnyAsync(u => u.Email == adminEmail || u.Username == adminUsername))
            {
                // Админ уже существует, ничего не делаем
                return;
            }

            // 3. Создаем нового админа
            var adminUser = new User
            {
                Email = adminEmail,
                Username = adminUsername,
                Role = "Admin", // <-- Устанавливаем роль "Admin"
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, adminPassword);

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();
            
            Console.WriteLine("Admin user created successfully.");
        }
    }
}