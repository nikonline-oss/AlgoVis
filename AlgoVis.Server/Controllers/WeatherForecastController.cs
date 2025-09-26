using AlgoVis.Server.Data;
using AlgoVis.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoVis.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("database")]
        public async Task<IActionResult> CheckDatabase()
        {
            try
            {
                // Проверяем, что база данных существует и доступна
                var canConnect = await _context.Database.CanConnectAsync();

                // Получаем информацию о таблицах
                var sessionsCount = await _context.Sessions.CountAsync();
                var stepsCount = await _context.Steps.CountAsync();

                // Получаем примененные миграции
                var migrations = await _context.Database.GetAppliedMigrationsAsync();

                return Ok(new
                {
                    DatabaseExists = canConnect,
                    SessionsCount = sessionsCount,
                    StepsCount = stepsCount,
                    AppliedMigrations = migrations.ToArray(),
                    Message = "✅ База данных работает корректно"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = ex.Message,
                    Message = "❌ Ошибка подключения к базе данных"
                });
            }
        }

        [HttpPost("test-session")]
        public async Task<IActionResult> CreateTestSession()
        {
            try
            {
                var session = new Models.VisualizationSession
                {
                    ClientConnectionId = "test-connection",
                    Code = "print('Hello, World!')",
                    Language = "python",
                    Status = "Ready"
                };

                _context.Sessions.Add(session);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    SessionId = session.Id,
                    Message = "✅ Тестовая сессия создана успешно"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = ex.Message,
                    Message = "❌ Ошибка при создании тестовой сессии"
                });
            }
        }
    }
}
