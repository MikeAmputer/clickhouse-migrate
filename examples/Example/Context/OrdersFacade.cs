﻿using ClickHouse.Facades;

namespace Example;

public class OrdersFacade : ClickHouseFacade<ExampleContext>
{
	#region InsertRandomOrders
	public async Task InsertRandomOrders(CancellationToken cancellationToken = default)
	{
		var sql = string.Format(InsertRandomOrdersSql, 100);

		await ExecuteNonQueryAsync(sql, cancellationToken);
	}

	private const string InsertRandomOrdersSql = """
		insert into example_orders

		select
			rand() % 10 + 1 as user_id,
			0 as order_id,
			toDecimal64(randUniform(100, 1000), 2) as price
		from
			numbers({0})
		""";
	#endregion

	#region GetTopExpensesUser
	public async Task<UserExpenses?> GetTopExpensesUser(CancellationToken cancellationToken = default)
	{
		var results = await ExecuteQueryAsync(
				GetTopExpensesUserSql,
				UserExpenses.FromReader,
				cancellationToken)
			.ToListAsync(cancellationToken);

		return results.SingleOrDefault();
	}

	private const string GetTopExpensesUserSql = """
		select
			user_id,
			expenses
		from
			example_user_total_expenses final
		order by
			expenses desc,
			user_id asc
		limit 1
		""";
	#endregion
}
