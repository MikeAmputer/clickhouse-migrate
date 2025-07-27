using ClickHouse.Facades;
using Microsoft.Extensions.Options;

namespace Example;

public class ExampleContextFactory : ClickHouseContextFactory<ExampleContext>
{
	private readonly string _connectionString;
	private readonly IClickHouseCommandExecutionListener _commandExecutionListener;
	private readonly IHttpClientFactory _httpClientFactory;

	public ExampleContextFactory(
		IHttpClientFactory httpClientFactory,
		IOptions<ClickHouseConfig> config,
		QueryLogger queryLogger)
	{
		ArgumentNullException.ThrowIfNull(httpClientFactory);
		ArgumentNullException.ThrowIfNull(config);
		ArgumentNullException.ThrowIfNull(queryLogger);

		_httpClientFactory = httpClientFactory;
		_connectionString = config.Value.ConnectionString;
		_commandExecutionListener = queryLogger;
	}

	protected override void SetupContextOptions(ClickHouseContextOptionsBuilder<ExampleContext> optionsBuilder)
	{
		optionsBuilder
			.WithConnectionString(_connectionString)
			.WithHttpClientFactory(_httpClientFactory, "ch-https")
			.WithCommandExecutionStrategy(CommandExecutionStrategy.Cancelable)
			.WithCommandExecutionListener(_commandExecutionListener);
	}
}
