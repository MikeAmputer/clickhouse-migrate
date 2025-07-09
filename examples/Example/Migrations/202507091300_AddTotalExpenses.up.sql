create table if not exists example_user_total_expenses (
	user_id			UInt32,
	expenses		Decimal64(6)
)
engine = SummingMergeTree
primary key user_id
order by user_id;

create materialized view if not exists example_user_total_expenses_mv
to example_user_total_expenses
as
select
	user_id,
	price as expenses
from example_orders;