using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using nomad_gis_V2.Data;
using nomad_gis_V2.Interfaces;
using nomad_gis_V2.Middleware;
using nomad_gis_V2.Models;
using nomad_gis_V2.Services;
using System.Text;
using Amazon.S3;
using Amazon.Runtime;
using Npgsql;
using NetTopologySuite;

var builder = WebApplication.CreateBuilder(args);

// ========== PostgreSQL ==========
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.UseNetTopologySuite()
    ));
// ========== JWT Authentication ==========
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Secret"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // для Render можно оставить false
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
    };
});

// ========== R2 Storage ==========
var r2Config = builder.Configuration.GetSection("R2Storage");
var credentials = new BasicAWSCredentials(r2Config["AccessKey"], r2Config["SecretKey"]);
var s3Config = new AmazonS3Config { ServiceURL = r2Config["ServiceURL"] };
builder.Services.AddSingleton<IAmazonS3>(new AmazonS3Client(credentials, s3Config));

// ========== DI ==========
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IMapPointService, MapPointService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IExperienceService, ExperienceService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

// ========== CORS ==========
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAdminPanel",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// ========== Controllers & Swagger ==========
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Nomad GIS API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Введите JWT токен так: Bearer {your token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

Console.WriteLine("Connection: " + builder.Configuration.GetConnectionString("DefaultConnection"));

// ========== Автоматическое применение миграций (Render-friendly) ==========
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();


    try
    {
        var config = services.GetRequiredService<IConfiguration>();
        if (db.Database.CanConnect())
        {
            var hasUsers = await db.Users.AnyAsync();
            if (!hasUsers)
            {
                await DataSeeder.SeedAdminUser(services, config);
            }
        }
        else
        {
            logger.LogWarning("⚠️ Таблица 'Users' не найдена, пропускаем сидинг.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying migrations or seeding data ❌");
    }
}


// ========== Middleware ==========
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAdminPanel");

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
