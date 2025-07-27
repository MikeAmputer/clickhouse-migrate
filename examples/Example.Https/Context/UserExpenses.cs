using System.Data.Common;

namespace Example;

public record UserExpenses(uint UserId,  decimal Expenses)
{
	public static UserExpenses FromReader(DbDataReader reader) =>
		new(reader.GetFieldValue<uint>(0), reader.GetDecimal(1));
}
