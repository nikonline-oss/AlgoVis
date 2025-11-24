using AlgoVis.Core.Core;
using AlgoVis.Evaluator.Evaluator.Types;
using AlgoVis.Server.Data;
using AlgoVis.Server.Hubs;
using AlgoVis.Server.Interfaces;
using AlgoVis.Server.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(wwwrootPath))
{
    Directory.CreateDirectory(wwwrootPath);
    Console.WriteLine($"Created wwwroot directory at: {wwwrootPath}");
}


// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));



builder.Services.AddSingleton<RandomStructureFactory>();

builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ICodeAnalysisService, CodeAnalysisService>();
builder.Services.AddScoped<IGigaChatService, GigaChatService>();
builder.Services.AddScoped<AlgorithmManager>();
builder.Services.AddScoped<AlgoVis.Core.Core.Interfaces.ICustomAlgorithmInterpreter, AlgoVis.Core.Core.AlgorithmInterpreter>();

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
        policy.WithOrigins("http://localhost:8000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
    });
});

var app = builder.Build();

// Инициализация БД ДО любого middleware, которое может её использовать
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        // Создаём БД и таблицы, если их нет
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Или используйте миграции (рекомендуется):
        // context.Database.Migrate();

        Console.WriteLine("Database created successfully!");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating the database.");
    }
}

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();
app.MapHub<VisualizationHub>("/visualizationHub");  // WebSocket

app.MapFallbackToFile("/index.html");

app.Run();
