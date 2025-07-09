using ClickHouse.Facades.Migrations;

namespace ClickHouse.Migrate;

public class MigrationInstructions : IClickHouseMigrationInstructions
{
	public string GetConnectionString() => _connectionString;
	public bool RollbackOnMigrationFail => _rollbackOnFail;

	private readonly string _connectionString;
	private readonly bool _rollbackOnFail;

	public MigrationInstructions(MigrationOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		_connectionString = options.GetConnectionString();
		_rollbackOnFail = options.RollbackOnFail ?? throw new InvalidOperationException("Invalid options.");
	}
}
