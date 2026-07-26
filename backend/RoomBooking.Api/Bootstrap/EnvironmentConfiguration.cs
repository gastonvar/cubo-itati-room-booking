using DotNetEnv;

namespace RoomBooking.Api.Bootstrap;

public static class EnvironmentConfiguration
{
    public static void Load(string contentRoot, IConfigurationManager configuration)
    {
        var envPath = FindEnvFile(contentRoot);
        if (envPath is not null)
            Env.Load(envPath);

        // CreateBuilder already loaded env vars; .env was applied after that,
        // so re-add the provider to pick up Section__Key bindings.
        configuration.AddEnvironmentVariables();
    }

    private static string? FindEnvFile(string startDir)
    {
        var dir = new DirectoryInfo(startDir);
        while (dir is not null)
        {
            var envFile = Path.Combine(dir.FullName, ".env");
            if (File.Exists(envFile))
                return envFile;
            dir = dir.Parent;
        }

        return null;
    }
}
