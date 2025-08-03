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

		if (log.FinalMigrationIndex != null)
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

	public static void LogMigrationStatements(this ILogger logger, IClickHouseMigrationLog log)
	{
		foreach (var logEntry in log.Entries)
		{
			if (logEntry.Success)
			{
				logger.LogInformation(
					"Migration {Index}_{Name} ({Direction}) applied successfully...",
					logEntry.Index,
					logEntry.Name,
					logEntry.Direction.ToString().ToLower());
			}
			else
			{
				logger.LogError(
					"An error occured during migration {Index}_{Name} ({Direction})...",
					logEntry.Index,
					logEntry.Name,
					logEntry.Direction.ToString().ToLower());
			}

			var doubleNewLine = Environment.NewLine + Environment.NewLine;

			var executedStatements = logEntry.ExecutedStatements.Any()
				? string.Join(
					doubleNewLine,
					logEntry.ExecutedStatements)
				: null;

			if (executedStatements != null)
			{
				logger.LogInformation($"Executed statements:{doubleNewLine}{executedStatements}{Environment.NewLine}");
			}

			if (logEntry.FailedStatement != null)
			{
				logger.LogError($"Failed statement:{doubleNewLine}{logEntry.FailedStatement}{Environment.NewLine}");
			}

			if (logEntry.Error != null)
			{
				logger.LogError(logEntry.Error);
			}
		}
	}
}
