using ClickHouse.Facades.Migrations;

namespace ClickHouse.Migrate;

public class MigrationsLocator : ClickHouseDirectoryMigrationsLocator
{
	protected override string DirectoryPath => _migrationsDirectory;

	private readonly string _migrationsDirectory;

	public MigrationsLocator(MigrationOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		_migrationsDirectory = options.MigrationsDirectory ?? throw new InvalidOperationException("Invalid options.");
	}
}
