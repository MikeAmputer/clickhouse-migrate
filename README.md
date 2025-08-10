# ch-migrate

[![Docker](https://img.shields.io/badge/docker-ch--migrate-blue?logo=docker)](https://hub.docker.com/r/mikeamputer/ch-migrate)
[![GHCR](https://img.shields.io/badge/ghcr.io-ch--migrate-blue?logo=github)](https://github.com/MikeAmputer/clickhouse-migrate/pkgs/container/ch-migrate)
[![NuGet](https://img.shields.io/nuget/v/ClickHouse.Migrate.Cli)](https://www.nuget.org/packages/ClickHouse.Migrate.Cli/)
[![License](https://img.shields.io/github/license/MikeAmputer/clickhouse-migrate)](https://github.com/MikeAmputer/clickhouse-migrate/blob/master/LICENSE)

Migrations tool for ClickHouse, distributed as a Docker image and .NET CLI tool. Built on top of the [ClickHouse.Facades](https://github.com/MikeAmputer/ClickHouse.Facades) .NET package, using HTTP client under the hood.

> [!NOTE]
> This is an unofficial tool and is not affiliated with or endorsed by ClickHouse Inc.
> 
> "ClickHouse" is a registered trademark of ClickHouse Inc. — [clickhouse.com](https://clickhouse.com/)

## Key Features
- Down migrations support
- Optional automatic rollback on migration fail
- HTTPS support
- Internal CA certificates support
- Support for both SQL and C# migrations (via `.cs` files, using `ClickHouse.Facades` notation)
- Conditional C# migrations based on ClickHouse server version

## Usage
Run the tool using Docker, specifying the desired command (`up` or `down`) and configuration options. The directory containing migration files should be mounted into the container as a volume.

### Commands
- `up`: Applies all pending migrations in the specified migrations directory.
- `down <index>`: Reverts migrations to the specified migration index, rolling back all newer migrations.

### Configuration Options
The tool supports configuration via command-line options or environment variables. Command-line options take precedence over environment variables. Boolean command-line arguments can not be used as flags — values should be provided explicitly `--https-enabled=true`.

| Option               | Environment Variable             | Description                                                                          | Default    |
|----------------------|----------------------------------|--------------------------------------------------------------------------------------|------------|
| `--host`             | `CH_MIGRATIONS_HOST`             | ClickHouse host.                                                                     | (Required) |
| `--port`             | `CH_MIGRATIONS_PORT`             | ClickHouse port.                                                                     | 8123       |
| `--user`             | `CH_MIGRATIONS_USER`             | ClickHouse user.                                                                     | (Required) |
| `--password`         | `CH_MIGRATIONS_PASSWORD`         | ClickHouse password.                                                                 | (Optional) |
| `--database`         | `CH_MIGRATIONS_DATABASE`         | ClickHouse database.                                                                 | (Required) |
| `--migrations-dir`   | `CH_MIGRATIONS_DIRECTORY`        | Directory containing migration SQL files.                                            | (Required) |
| `--timeout-sec`      | `CH_MIGRATIONS_TIMEOUT`          | Command timeout in seconds.                                                          | 60         |
| `--https-enabled`    | `CH_MIGRATIONS_HTTPS_ENABLED`    | Use HTTPS connection (`true`/`false`).                                               | `false`    |
| `--rollback-on-fail` | `CH_MIGRATIONS_ROLLBACK_ON_FAIL` | Automatically rollback on migration failure using the `down` migration if available. | `false`    |
| `--verbose`          | `CH_MIGRATIONS_VERBOSE`          | Enable verbose logging.                                                              | `true`     |

### Migration Files
Migration files must be placed in the directory specified by `--migrations-dir` (or `CH_MIGRATIONS_DIRECTORY`) and follow a naming convention such as:
```
0001_Initial.up.sql
0001_Initial.down.sql
```
Each filename must start with a migration index like `0001_`, followed by migration name (underscores `_` are allowed), suffixed with the migration direction `.up` or `.down`, and ending with the `.sql` file extension. **Down migrations are optional.**

Migration files are split by the semicolon `;` into individual SQL statements. Since ClickHouse does not support executing multiple statements in a single query, each statement is executed separately. All statements in a migration are run within a session, so session-scoped features like temporary tables are supported. However, migrations are not executed within a transaction — if a failure occurs mid-migration, earlier statements will not be automatically rolled back. Use the `--rollback-on-fail` option to enable automatic rollback if needed.

### Quick Setup

bash:
```bash
docker run --rm \
  -e CH_MIGRATIONS_HOST="example.clickhouse.host" \
  -e CH_MIGRATIONS_PORT="8123" \
  -e CH_MIGRATIONS_USER="example_user" \
  -e CH_MIGRATIONS_PASSWORD="example_password" \
  -e CH_MIGRATIONS_DATABASE="example_db" \
  -e CH_MIGRATIONS_DIRECTORY="/scripts" \
  -v "$(pwd)/migrations:/scripts" \
  mikeamputer/ch-migrate:latest up
``` 

PowerShell:
```powershell
docker run --rm `
  -e CH_MIGRATIONS_HOST="example.clickhouse.host" `
  -e CH_MIGRATIONS_PORT="8123" `
  -e CH_MIGRATIONS_USER="example_user" `
  -e CH_MIGRATIONS_PASSWORD="example_password" `
  -e CH_MIGRATIONS_DATABASE="example_db" `
  -e CH_MIGRATIONS_DIRECTORY="/scripts" `
  -v "${PWD}\migrations:/scripts" `
  mikeamputer/ch-migrate:latest up
```

Docker Compose:
```yaml
ch-migrate:
  image: mikeamputer/ch-migrate:latest
  environment:
    - CH_MIGRATIONS_HOST=example.clickhouse.host
    - CH_MIGRATIONS_PORT=8123
    - CH_MIGRATIONS_USER=example_user
    - CH_MIGRATIONS_PASSWORD=example_password
    - CH_MIGRATIONS_DATABASE=example_db
    - CH_MIGRATIONS_DIRECTORY=/scripts
  volumes:
    - ./migrations:/scripts
  command: up
```

`docker-compose` does not support `--rm` (auto-remove) as part of the YAML service definition.

For an example using `healthcheck`, see [this docker-compose example](https://github.com/MikeAmputer/clickhouse-migrate/blob/master/examples/Example/docker-compose.yml).

### Using a Custom CA Certificate

To enable HTTPS connections with a self-signed or internal CA certificate, mount a volume containing your `.crt` file into the container at `/usr/local/share/ca-certificates` (or mount a single `.crt` file directly). The certificate will be automatically installed during container startup.

```yaml
volumes:
  - ca:/usr/local/share/ca-certificates:ro
```

Make sure the mounted directory contains valid `.crt` files and file permissions are set to `chmod 644`. These will be registered with the container's trusted store using `update-ca-certificates`.

An example setup can be found in the [Example.Https directory](https://github.com/MikeAmputer/clickhouse-migrate/tree/master/examples/Example.Https).

### .NET CLI Tool
As an alternative to Docker, `ch-migrate` is also available as a .NET CLI tool via NuGet (`NET 8` or `NET 9` required):

```
dotnet tool install --global ClickHouse.Migrate.Cli
```

To update the template, use the `update` subcommand. To uninstall the template, use the `uninstall` subcommand.

Once installed, run the tool using the ch-migrate command:

```
ch-migrate up --host localhost --user example_user --database example_db --migrations-dir ./migrations
```

You can also use environment variables for configuration (see table above).

### C# Migrations
In addition to raw SQL files, `ch-migrate` supports C#-based migrations by using `.cs` files. These migrations are compiled at runtime and executed using the `ClickHouse.Facades` library.

To write a C# migration, create a `.cs` file in your migrations directory. Each file must contain a class that inherits from `ClickHouseMigration` and is annotated with the `[ClickHouseMigration]` attribute, specifying a unique migration index and a descriptive name.

```c#
[ClickHouseMigration(101, "MyMigration")]
public class MyMigrationClass : ClickHouseMigration
{
	protected override void Up(ClickHouseMigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddRawSqlStatement("create table...");
	}

	protected override void Down(ClickHouseMigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddRawSqlStatement("drop table...");
	}
}
```

You can conditionally execute logic based on the ClickHouse server version.

For a full working example, see [this migration class](https://github.com/MikeAmputer/clickhouse-migrate/blob/master/examples/Example/Migrations/202507091600_AddGaugeMetrics.cs). For detailed API documentation, refer to the [ClickHouse.Facades migration guide](https://github.com/MikeAmputer/ClickHouse.Facades/wiki/Migrations#migration-class).
