using ClickHouse.Facades;
using ClickHouse.Facades.Migrations;
using ClickHouse.Migrate;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CommandLine;

var optionsResult = Parser.Default.ParseArguments<UpOptions, DownOptions>(args);

return await optionsResult.MapResult(
	async (UpOptions options) =>
	{
		var host = CreateHostBuilder(args, options).Build();
		var serviceProvider = host.Services;

		var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

		logger.LogAppVersion();
		logger.LogInformation("Command: up");

		try
		{
			var migrationLog = await serviceProvider.ClickHouseMigrateAsync();

			logger.LogMigrationEssentials(migrationLog);
			logger.LogInformation("Database schema is up to date");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An error occured during 'up' command execution");

			return ErrorCodes.CommandExecutionError;
		}

		return 0;
	},
	async (DownOptions options) =>
	{
		var host = CreateHostBuilder(args, options).Build();
		var serviceProvider = host.Services;

		var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

		logger.LogAppVersion();
		logger.LogInformation("Command: down {Index}", options.Index);

		try
		{
			var migrationLog = await serviceProvider.ClickHouseRollbackAsync(options.Index);

			logger.LogMigrationEssentials(migrationLog);
			logger.LogInformation("Database schema was rolled back to {Index} state", options.Index);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "An error occured during 'down {Index}' command execution", options.Index);

			return ErrorCodes.CommandExecutionError;
		}

		return 0;
	},
	errors =>
	{
		var host = CreateHostBuilder(args, null).Build();
		var serviceProvider = host.Services;

		var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

		logger.LogAppVersion();

		foreach (var error in errors)
		{
			logger.LogError("Error while parsing command line arguments: {ErrorTag}", error.Tag.ToString());
		}

		return Task.FromResult(ErrorCodes.InvalidCommandLineArguments);
	});

static IHostBuilder CreateHostBuilder(string[] args, MigrationOptions? options) =>
	Host.CreateDefaultBuilder(args)
		.ConfigureLogging(logging =>
		{
			logging
				.ClearProviders()
				.AddConsoleFormatter<CustomConsoleFormatter, CustomConsoleFormatterOptions>()
				.AddConsole(options => options.FormatterName = nameof(CustomConsoleFormatter));
#if DEBUG
			logging.SetMinimumLevel(LogLevel.Debug);
#else
			logging.SetMinimumLevel(LogLevel.Information);
#endif
		})
		.ConfigureServices((_, services) =>
		{
			if (options is null)
			{
				return;
			}

			services.AddSingleton<MigrationOptions>(serviceProvider =>
				{
					var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
					var optionsAreValid = options.ApplyEnvironmentFallback(logger);

					if (!optionsAreValid)
					{
						Environment.Exit(ErrorCodes.InvalidOptions);
					}

					return options;
				});

			services.AddClickHouseMigrations<MigrationInstructions, MigrationsLocator>();
		});
