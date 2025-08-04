[ClickHouseMigration(202507091600, "AddGaugeMetrics")]
public class AddGaugeMetrics_Migration : ClickHouseMigration
{
	protected override void Up(ClickHouseMigrationBuilder migrationBuilder)
	{
		migrationBuilder.SinceVersion("23.3", mb =>
		{
			mb.AddRawSqlStatement("""
			create table if not exists metrics_gauge (
				name				LowCardinality(String),
				labels				Map(LowCardinality(String), String),
				help				LowCardinality(String),
				value				Int64,
				date_time			DateTime('UTC')			materialized now('UTC'),
				ver					UInt32					materialized toUnixTimestamp(date_time),
			)
			engine = ReplacingMergeTree(ver)
			primary key (name, labels)
			order by (name, labels)
			""");
		});
	}

	protected override void Down(ClickHouseMigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddRawSqlStatement("drop table if exists metrics_gauge");
	}
}
