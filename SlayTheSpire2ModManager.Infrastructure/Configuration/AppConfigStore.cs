using Microsoft.EntityFrameworkCore;

namespace SlayTheSpire2ModManager.Infrastructure.Configuration;

public class AppConfigStore
{
    private const string GameDirectoryKey = "GameDirectory";
    private readonly DbContextOptions<AppConfigDbContext> _dbContextOptions;

    public AppConfigStore(string? dbPath = null)
    {
        dbPath ??= GetDefaultDbPath();

        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        _dbContextOptions = new DbContextOptionsBuilder<AppConfigDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        using var dbContext = new AppConfigDbContext(_dbContextOptions);
        dbContext.Database.EnsureCreated();
    }

    public async Task<string?> GetGameDirectoryAsync(CancellationToken cancellationToken = default)
    {
        return await GetValueAsync(GameDirectoryKey, cancellationToken);
    }

    public async Task SetGameDirectoryAsync(string gameDirectory, CancellationToken cancellationToken = default)
    {
        await SetValueAsync(GameDirectoryKey, gameDirectory, cancellationToken);
    }

    public async Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default)
    {
        await using var dbContext = new AppConfigDbContext(_dbContextOptions);

        var entity = await dbContext.AppConfigEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Key == key, cancellationToken);

        return entity?.Value;
    }

    public async Task SetValueAsync(string key, string? value, CancellationToken cancellationToken = default)
    {
        await using var dbContext = new AppConfigDbContext(_dbContextOptions);

        var entity = await dbContext.AppConfigEntries
            .FirstOrDefaultAsync(x => x.Key == key, cancellationToken);

        if (entity is null)
        {
            dbContext.AppConfigEntries.Add(new AppConfigEntry { Key = key, Value = value });
        }
        else
        {
            entity.Value = value;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string GetDefaultDbPath()
    {
        var appDataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SlayTheSpire2ModManager");

        return Path.Combine(appDataDir, "config.db");
    }
}
