# ch-migrate
[![License](https://img.shields.io/github/license/MikeAmputer/clickhouse-migrate)](https://github.com/MikeAmputer/clickhouse-migrate/blob/master/LICENSE)

Migrations tool for ClickHouse, distributed as a Docker image. Built on top of the [ClickHouse.Facades](https://github.com/MikeAmputer/ClickHouse.Facades) .NET package, using HTTP client under the hood.

> [!NOTE]
> This is an unofficial tool and is not affiliated with or endorsed by ClickHouse Inc.
> 
> "ClickHouse" is a registered trademark of ClickHouse Inc. — [clickhouse.com](https://clickhouse.com/)

## Key Features
- Down migrations support
- Optional automatic rollback on migration fail
- HTTPS support

## Usage
Run the tool using Docker, specifying the desired command (`up` or `down`) and configuration options.

### Commands
- `up`: Applies all pending migrations in the specified migrations directory.
- `down <index>`: Reverts migrations to the specified migration index, rolling back all newer migrations.

### Configuration Options
The tool supports configuration via command-line options or environment variables. Command-line options take precedence over environment variables. Boolean command-line arguments can not be used as flags - values should be provided explicitly `--https-enabled=true`.

| Option                | Environment Variable           | Description                                    | Default           |
|-----------------------|--------------------------------|------------------------------------------------|-------------------|
| `--host`              | `CH_MIGRATIONS_HOST`           | ClickHouse host.                               | (Required)        |
| `--port`              | `CH_MIGRATIONS_PORT`           | ClickHouse port.                               | 8123              |
| `--user`              | `CH_MIGRATIONS_USER`           | ClickHouse user.                               | (Required)        |
| `--password`          | `CH_MIGRATIONS_PASSWORD`       | ClickHouse password.                           | (Optional)        |
| `--database`          | `CH_MIGRATIONS_DATABASE`       | ClickHouse database.                           | (Required)        |
| `--migrations-dir`    | `CH_MIGRATIONS_DIRECTORY`      | Directory containing migration SQL files.      | (Required)        |
| `--timeout-sec`       | `CH_MIGRATIONS_TIMEOUT`        | Command timeout in seconds.                    | 60                |
| `--https-enabled`     | `CH_MIGRATIONS_HTTPS_ENABLED`  | Use HTTPS connection (`true`/`false`).         | `false`           |
| `--rollback-on-fail`  | `CH_MIGRATIONS_ROLLBACK_ON_FAIL` | Automatically rollback on migration failure. | `false`           |

### Migration Files
Migration files must be placed in the directory specified by `--migrations-dir` (or `CH_MIGRATIONS_DIRECTORY`) and follow a naming convention such as:
```
0001_Initial.up.sql
0001_Initial.down.sql
```
Each filename must start with a migration index like `0001_`, followed by migration name (underscores `_` are allowed), suffixed with the migration direction `.up` or `.down`, and ending with the `.sql` file extension. **Down migrations are optional.**
