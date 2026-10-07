using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using AlgoVis.Data;
using System.Threading.RateLimiting;
using AlgoVis.Server.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

// ─────── Database ───────
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("ConnectionStrings:Default is required");
builder.Services.AddAlgoVisData(connectionString);

// ─────── Services ───────
builder.Services.Configure<AlgoVis.Server.Auth.JwtOptions>(
    builder.Configuration.GetSection(AlgoVis.Server.Auth.JwtOptions.SectionName));
builder.Services.AddSingleton<AlgoVis.Server.Auth.Services.PasswordHasher>();
builder.Services.AddSingleton<AlgoVis.Server.Auth.Services.JwtService>();
builder.Services.AddScoped<AlgoVis.Server.Auth.Services.AuthService>();
builder.Services.AddScoped<AlgoVis.Server.Projects.Services.ProjectsService>();

// ─────── JWT Authentication ───────
{
    var jwtSection = builder.Configuration.GetSection(AlgoVis.Server.Auth.JwtOptions.SectionName);
    var secret = jwtSection["Secret"] ?? throw new InvalidOperationException("Jwt:Secret is required");
    var issuer = jwtSection["Issuer"] ?? "AlgoVis";
    var audience = jwtSection["Audience"] ?? "AlgoVis.Client";

    builder.Services.AddAuthentication(
        Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opts =>
        {
            opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(secret)),
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });
    builder.Services.AddAuthorization();
}

var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
    Console.WriteLine($"Created wwwroot directory at: {wwwrootPath}");
}



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });


// Add SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
    options.KeepAliveInterval = TimeSpan.FromSeconds(30);
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
            "http://localhost",           // nginx порт 80
            "http://127.0.0.1",           // nginx порт 80  
            "http://81.94.156.231",        // ваш внешний IP
            "http://localhost:3000"
        )
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
    });
});

// ─────── Rate limiting ───────
builder.Services.Configure<RateLimitOptions>(
    builder.Configuration.GetSection(RateLimitOptions.SectionName));

var rateLimitSection = builder.Configuration.GetSection(RateLimitOptions.SectionName);
var authRunLimit = rateLimitSection.GetValue<int?>("AuthenticatedRunPerMinute") ?? 30;
var anonRunLimit = rateLimitSection.GetValue<int?>("AnonymousRunPerMinute") ?? 10;
var registerLimit = rateLimitSection.GetValue<int?>("RegisterPerHour") ?? 5;
var loginLimit = rateLimitSection.GetValue<int?>("LoginPerMinute") ?? 10;

builder.Services.AddRateLimiter(opts =>
{
    opts.RejectionStatusCode = 429;

    // Для запуска кода: разные лимиты для авторизованных и анонимов
    opts.AddPolicy("run", ctx =>
    {
        var isAuth = ctx.User.Identity?.IsAuthenticated == true;
        var key = isAuth
            ? "u:" + (ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                   ?? ctx.User.FindFirst("sub")?.Value
                   ?? "unknown")
            : "ip:" + (ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown");

        var limit = isAuth ? authRunLimit : anonRunLimit;

        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = limit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });

    // Регистрация: жёстко по IP
    opts.AddPolicy("register", ctx =>
    {
        var key = "ip:" + (ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = registerLimit,
            Window = TimeSpan.FromHours(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });

    // Логин: по IP
    opts.AddPolicy("login", ctx =>
    {
        var key = "ip:" + (ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown");
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = loginLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    });
});

var app = builder.Build();

// Автоматически создаём/обновляем схему БД при запуске.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AlgoVis.Data.AlgoVisDbContext>();
    db.Database.EnsureCreated();
}

app.UseAuthentication();
app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
// app.UseHttpsRedirection();    
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.Run();
//ASPNETCORE_HOSTINGSTARTUPASSEMBLIES="" dotnet watch run --project AlgoVis.Server