create table if not exists example_orders (
	user_id			UInt32,
	order_id		UInt64,
	price			Decimal64(6)
)
engine = MergeTree
primary key (user_id, order_id)
order by (user_id, order_id);