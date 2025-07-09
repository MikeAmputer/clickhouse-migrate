using System.Reflection;
using Microsoft.Extensions.Logging;

namespace ClickHouse.Migrate;

public static class LoggerExtensions
{
	public static void LogAppVersion(this ILogger logger)
	{
		logger.LogInformation("App Version: {Version}", Assembly.GetEntryAssembly()?.GetName().Version?.ToString());
	}
}
