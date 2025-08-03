using CommandLine;
using Microsoft.Extensions.Logging;

namespace ClickHouse.Migrate;

public abstract class MigrationOptions
{
	[Option("host", Required = false, HelpText = "ClickHouse host.")]
	public string? Host { get; set; }

	[Option("port", Required = false, HelpText = "ClickHouse port.")]
	public int? Port { get; set; }

	[Option("user", Required = false, HelpText = "ClickHouse user.")]
	public string? User { get; set; }

	[Option("password", Required = false, HelpText = "ClickHouse password.")]
	public string? Password { get; set; }

	[Option("database", Required = false, HelpText = "ClickHouse database.")]
	public string? Database { get; set; }

	[Option("migrations-dir", Required = false, HelpText = "Directory containing migration SQL files.")]
	public string? MigrationsDirectory { get; set; }

	[Option("timeout-sec", Required = false, HelpText = "Command timeout in seconds.")]
	public int? TimeoutSeconds { get; set; }

	[Option("https-enabled", Required = false, HelpText = "Use HTTPS connection.")]
	public bool? HttpsEnabled { get; set; }

	[Option("rollback-on-fail", Required = false, HelpText = "Automatically rollback a migration if it fails.")]
	public bool? RollbackOnFail { get; set; }

	[Option("verbose", Required = false, HelpText = "Enable verbose logging.")]
	public bool? Verbose { get; set; }

	public bool ApplyEnvironmentFallback(ILogger logger)
	{
		Host ??= TryGetRequired("CH_MIGRATIONS_HOST", logger);
		Port ??= TryParseInt("CH_MIGRATIONS_PORT") ?? 8123;
		User ??= TryGetRequired("CH_MIGRATIONS_USER", logger);
		Password ??= Environment.GetEnvironmentVariable("CH_MIGRATIONS_PASSWORD");
		Database ??= TryGetRequired("CH_MIGRATIONS_DATABASE", logger);
		MigrationsDirectory ??= TryGetRequired("CH_MIGRATIONS_DIRECTORY", logger);
		TimeoutSeconds ??= TryParseInt("CH_MIGRATIONS_TIMEOUT") ?? 60;
		HttpsEnabled ??= TryParseBool("CH_MIGRATIONS_HTTPS_ENABLED") ?? false;
		RollbackOnFail ??= TryParseBool("CH_MIGRATIONS_ROLLBACK_ON_FAIL") ?? false;
		Verbose ??= TryParseBool("CH_MIGRATIONS_VERBOSE") ?? true;

		return Host is not null
			&& User is not null
			&& Database is not null
			&& MigrationsDirectory is not null;
	}

	public string GetConnectionString()
	{
		_ = Host ?? throw new InvalidOperationException($"{nameof(Host)} is null.");
		_ = Port ?? throw new InvalidOperationException($"{nameof(Port)} is null.");
		_ = Database ?? throw new InvalidOperationException($"{nameof(Database)} is null.");
		_ = User ?? throw new InvalidOperationException($"{nameof(User)} is null.");

		var result = $"host={Host};port={Port};database={Database};username={User};";

		if (Password is not null)
		{
			result += $"password={Password};";
		}

		if (TimeoutSeconds is not null)
		{
			result += $"Timeout={TimeoutSeconds};";
		}

		if (HttpsEnabled ?? false)
		{
			result += "Protocol=https;";
		}

		return result;
	}

	private static int? TryParseInt(string envVar)
	{
		var val = Environment.GetEnvironmentVariable(envVar);

		return int.TryParse(val, out var result) ? result : null;
	}

	private static bool? TryParseBool(string envVar)
	{
		var val = Environment.GetEnvironmentVariable(envVar);

		return bool.TryParse(val, out var result) ? result : null;
	}

	private static string? TryGetRequired(string envVar, ILogger logger)
	{
		var val = Environment.GetEnvironmentVariable(envVar);

		if (val is null)
		{
			logger.LogError("Missing options variable {EnvVar}", envVar);
		}

		return val;
	}
}

[Verb("up", HelpText = "Apply all pending migrations.")]
public class UpOptions : MigrationOptions
{
}

[Verb("down", HelpText = "Revert to a specific migration index.")]
public class DownOptions : MigrationOptions
{
	[Value(0, Required = true, HelpText = "Target migration index. All newer migrations will be rolled back.")]
	public ulong Index { get; set; }
}
