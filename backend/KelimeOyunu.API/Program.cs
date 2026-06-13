using System.Text;
using KelimeOyunu.API.Hubs;
using KelimeOyunu.Application.Services;
using KelimeOyunu.API.Services;
using KelimeOyunu.Core.Interfaces;
using KelimeOyunu.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Redis (StackExchange.Redis)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "KelimeOyunu_";
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "KelimeOyunuSuperSecretKey2026!@#$%^&*()";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "KelimeOyunu",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "KelimeOyunuApp",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        // SignalR için query string'den token alma
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/hubs/game") || path.StartsWithSegments("/hubs/chat")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// CORS (Frontend için)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// DI — SOLID: Dependency Inversion Principle
builder.Services.AddScoped<IGameManager, GameManager>();
builder.Services.AddSingleton<ISessionManager, SessionManager>();
builder.Services.AddScoped<IValidationEngine, ValidationEngine>();
builder.Services.AddScoped<IEconomyManager, EconomyManager>();
builder.Services.AddScoped<ISocialManager, SocialManager>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IQuestManager, QuestManager>();
builder.Services.AddScoped<KelimeOyunu.Core.Interfaces.IQuestNotificationService, KelimeOyunu.API.Services.QuestNotificationService>();
builder.Services.AddHttpClient<KelimeOyunu.Core.Interfaces.IAIService, KelimeOyunu.Infrastructure.Services.GeminiAIService>();
builder.Services.AddSingleton<IBotManager, BotManager>();

// SignalR
builder.Services.AddSignalR();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger (Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<GameHub>("/hubs/game");
app.MapHub<ChatHub>("/hubs/chat");

// Auto-migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// JSON Seed Data
await KelimeOyunu.API.SeedData.DbInitializer.SeedQuestionsFromJsonAsync(app);

app.Run();
