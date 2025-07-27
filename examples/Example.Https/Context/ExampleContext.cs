using ClickHouse.Facades;

namespace Example;

public class ExampleContext : ClickHouseContext<ExampleContext>
{
	public OrdersFacade Orders => GetFacade<OrdersFacade>();
}
