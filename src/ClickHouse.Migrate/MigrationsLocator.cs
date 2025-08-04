using ClickHouse.Facades.Migrations;

namespace ClickHouse.Migrate;

public class MigrationsLocator(MigrationOptions options) : AggregateClickHouseMigrationsLocator
{
	private readonly MigrationOptions _options = options ?? throw new InvalidOperationException("Invalid options.");

	protected override IEnumerable<IClickHouseMigrationsLocator> Locators =>
	[
		new SqlMigrationsLocator(_options),
		new RoslynMigrationsLocator(_options),
	];
}
