using AlgoVis.Core.Core;
using AlgoVis.Evaluator.Evaluator.Types;
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



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddScoped<RandomStructureFactory>();
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

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");      
app.UseHttpsRedirection();    
app.UseAuthorization();       
app.MapControllers();



app.Run();
