namespace SlayTheSpire2ModManager.Infrastructure.Configuration;

public class AppConfigEntry
{
    public int Id { get; set; }

    public required string Key { get; set; }

    public string? Value { get; set; }
}
