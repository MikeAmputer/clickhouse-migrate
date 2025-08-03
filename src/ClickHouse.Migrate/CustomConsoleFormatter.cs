using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace ClickHouse.Migrate;

public class CustomConsoleFormatter() : ConsoleFormatter(nameof(CustomConsoleFormatter))
{
	public override void Write<TState>(
		in LogEntry<TState> logEntry,
		IExternalScopeProvider? scopeProvider,
		TextWriter textWriter)
	{
		var message = logEntry.Formatter(logEntry.State, logEntry.Exception);
		if (string.IsNullOrEmpty(message) && logEntry.Exception == null)
		{
			return;
		}

		var color = logEntry.LogLevel switch
		{
			LogLevel.Information => ConsoleColor.White,
			LogLevel.Warning => ConsoleColor.Yellow,
			LogLevel.Error => ConsoleColor.Red,
			LogLevel.Critical => ConsoleColor.Red,
			_ => ConsoleColor.White
		};

		Console.ForegroundColor = color;

		Console.WriteLine(
			$"[{logEntry.LogLevel}] {message}");

		Console.ResetColor();
	}
}

public class CustomConsoleFormatterOptions : ConsoleFormatterOptions;
