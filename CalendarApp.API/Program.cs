using CalendarApp.API.Data;
using CalendarApp.API.Data.Repositories.Implementations;
using CalendarApp.API.Data.Repositories.Interfaces;
using CalendarApp.API.Services;
using CalendarApp.API.Services.Implementations;
using CalendarApp.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;

// Configure Npgsql to convert DateTime to UTC automatically
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger configuration
builder.Services.AddSwaggerGen();

// Database Configuration - Supports both SQL Server and PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Try DATABASE_URL first (Railway's default), then fall back to ConnectionStrings:DefaultConnection
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? builder.Configuration.GetConnectionString("DefaultConnection");

    // Trim any whitespace that might cause parsing issues
    connectionString = connectionString?.Trim();

    Console.WriteLine($"[DEBUG] Connection string source: {(Environment.GetEnvironmentVariable("DATABASE_URL") != null ? "DATABASE_URL env var" : "ConnectionStrings:DefaultConnection")}");
    Console.WriteLine($"[DEBUG] Connection string length: {connectionString?.Length ?? 0}");
    Console.WriteLine($"[DEBUG] Connection string starts with: {connectionString?.Substring(0, Math.Min(20, connectionString?.Length ?? 0)) ?? "NULL"}...");

    // Print full connection string with password obscured
    if (!string.IsNullOrEmpty(connectionString))
    {
        var obscured = System.Text.RegularExpressions.Regex.Replace(
            connectionString,
            @"://([^:]+):([^@]+)@",
            "://$1:****@"
        );
        Console.WriteLine($"[DEBUG] Full connection string (obscured): {obscured}");

        // Check for hidden characters
        Console.WriteLine($"[DEBUG] First char code: {(int)connectionString[0]}");
        Console.WriteLine($"[DEBUG] Has whitespace at start: {char.IsWhiteSpace(connectionString[0])}");
    }

    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Database connection string is not configured");
    }

    // Use PostgreSQL if connection string starts with postgres/postgresql
    if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
        connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        // Parse PostgreSQL URI into Npgsql connection string format
        // URI format: postgresql://user:password@host:port/database
        try
        {
            var uri = new Uri(connectionString.Replace("postgres://", "postgresql://"));
            var npgsqlConnectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]}";

            Console.WriteLine("[DEBUG] Parsed Npgsql connection string (obscured): " + npgsqlConnectionString.Replace(uri.UserInfo.Split(':')[1], "****"));
            Console.WriteLine("[DEBUG] Using PostgreSQL with Npgsql");

            options.UseNpgsql(npgsqlConnectionString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to parse PostgreSQL URI: {ex.Message}");
            throw;
        }
    }
    else
    {
        // Use SQL Server for local development or Azure SQL
        Console.WriteLine("[DEBUG] Using SQL Server");
        options.UseSqlServer(connectionString);
    }
});

// Redis Configuration - TEMPORARILY DISABLED for quick start
// Uncomment when Redis is installed
/*
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetSection("Redis:Configuration").Value
        ?? throw new InvalidOperationException("Redis configuration is missing");
    return ConnectionMultiplexer.Connect(configuration);
});
*/
// Dummy cache service for now
builder.Services.AddSingleton<ICacheService, DummyCacheService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000", "http://localhost:5173" };

        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Repository Registration
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ICalendarShareRepository, CalendarShareRepository>();

// Service Registration
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<ICacheService, DummyCacheService>();

// RabbitMQ and Email Services
builder.Services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

// Auto-run migrations in production
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Running database migrations...");

        // Apply pending migrations (this will also create the database if it doesn't exist)
        logger.LogInformation("Applying pending migrations...");
        var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
        logger.LogInformation($"Found {pendingMigrations.Count} pending migrations");

        dbContext.Database.Migrate();
        logger.LogInformation("Database migrations completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
        throw;
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Calendar App API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
