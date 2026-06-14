using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProdigeeTutsPoint.Infrastructure.Content;
using ProdigeeTutsPoint.Infrastructure.Persistence;

namespace ProdigeeTutsPoint.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ContentOptions>(configuration.GetSection("Content"));

        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=App_Data/prodigee-tuts-point.sqlite";
        EnsureSqliteDirectoryExists(connectionString);

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<ContentFileReader>();
        services.AddScoped<ContentIndexingService>();
        services.AddHostedService<ContentIndexingHostedService>();

        return services;
    }

    private static void EnsureSqliteDirectoryExists(string connectionString)
    {
        const string dataSourcePrefix = "Data Source=";
        var dataSourcePart = connectionString
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault(part => part.StartsWith(dataSourcePrefix, StringComparison.OrdinalIgnoreCase));

        if (dataSourcePart is null)
        {
            return;
        }

        var databasePath = dataSourcePart[dataSourcePrefix.Length..];
        var directory = Path.GetDirectoryName(Path.GetFullPath(databasePath));

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
