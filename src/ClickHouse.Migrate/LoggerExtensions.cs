using System.Reflection;
using ClickHouse.Facades.Migrations;
using Microsoft.Extensions.Logging;

namespace ClickHouse.Migrate;

public static class LoggerExtensions
{
	public static void LogAppVersion(this ILogger logger)
	{
		logger.LogInformation("App Version: {Version}", Assembly.GetEntryAssembly()?.GetName().Version?.ToString());
	}

	public static void LogMigrationEssentials(this ILogger logger, IClickHouseMigrationLog log)
	{
		if (log.InitialMigrationIndex != null)
		{
			logger.LogInformation(
				"Initial database state: {InitialMigrationIndex}_{InitialMigrationName}",
				log.InitialMigrationIndex,
				log.InitialMigrationName);
		}
		else
		{
			logger.LogInformation("Initial database state: no migrations applied");
		}

		if (log.Success)
		{
			logger.LogInformation("Command executed successfully");
		}
		else
		{
			logger.LogError("Command execution failed");
		}

		if (log.InitialMigrationIndex != null)
		{
			logger.LogInformation(
				"Current database state: {FinalMigrationIndex}_{FinalMigrationName}",
				log.FinalMigrationIndex,
				log.FinalMigrationName);
		}
		else
		{
			logger.LogInformation("Current database state: no migrations applied");
		}
	}
}
