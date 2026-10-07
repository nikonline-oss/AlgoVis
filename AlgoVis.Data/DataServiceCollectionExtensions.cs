using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AlgoVis.Data;

public static class DataServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует DbContext на PostgreSQL.
    /// </summary>
    public static IServiceCollection AddAlgoVisData(
        this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AlgoVisDbContext>(opts =>
            opts.UseNpgsql(connectionString));
        return services;
    }
}
