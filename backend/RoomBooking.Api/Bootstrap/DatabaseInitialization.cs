using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RoomBooking.Api.Features.Auth.Repositories;
using RoomBooking.Api.Features.Rooms.Repositories;
using RoomBooking.Api.Shared.Data;

namespace RoomBooking.Api.Bootstrap;

public static class DatabaseInitialization
{
    // SQLITE_BUSY — see https://www.sqlite.org/rescode.html#busy
    private const int SqliteBusy = 5;

    public static async Task InitializeAsync(WebApplication app)
    {
        Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "Data"));

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Startup");

        try
        {
            await MigrateWithBusyRetryAsync(db, logger);
        }
        catch (Exception ex) when (IsLegacySchemaWithoutMigrations(ex))
        {
            // DBs created with EnsureCreated lack __EFMigrationsHistory — recreate once.
            logger.LogWarning(ex, "Migrate failed; recreating SQLite database for schema upgrade");
            await db.Database.CloseConnectionAsync();
            // Pooled connections keep the file locked; clear before delete/recreate.
            SqliteConnection.ClearAllPools();
            await db.Database.EnsureDeletedAsync();
            await MigrateWithBusyRetryAsync(db, logger);
        }

        var rooms = scope.ServiceProvider.GetRequiredService<IRoomRepository>();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        await DbSeeder.SeedAsync(rooms, users);
    }

    // Retries MigrateAsync on SQLITE_BUSY so a transient lock does not fail startup.
    private static async Task MigrateWithBusyRetryAsync(AppDbContext db, ILogger logger)
    {
        const int maxAttempts = 5;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                return;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == SqliteBusy && attempt < maxAttempts)
            {
                var delayMs = 100 * attempt;
                logger.LogWarning(
                    ex,
                    "SQLite busy during migrate (attempt {Attempt}/{Max}); retrying in {DelayMs}ms",
                    attempt,
                    maxAttempts,
                    delayMs);
                await Task.Delay(delayMs);
            }
        }
    }

    // True when migrate failed because the DB was created outside EF migrations (e.g. EnsureCreated).
    // False for SQLITE_BUSY so we never delete a locked database.
    private static bool IsLegacySchemaWithoutMigrations(Exception ex)
    {
        for (var current = ex; current is not null; current = current.InnerException)
        {
            if (current is SqliteException { SqliteErrorCode: SqliteBusy })
                return false;
        }

        var text = ex.ToString();
        return text.Contains("already exists", StringComparison.OrdinalIgnoreCase)
            || text.Contains("no such table", StringComparison.OrdinalIgnoreCase)
            || text.Contains("__EFMigrationsHistory", StringComparison.OrdinalIgnoreCase);
    }
}
