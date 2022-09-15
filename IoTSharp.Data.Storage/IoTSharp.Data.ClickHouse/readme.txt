docker run --rm -d --name=clickhouse-server --ulimit nofile=262144:262144 -p 8123:8123 -p 9009:9009 -p 9090:9000 yandex/clickhouse-server:20.3.5.21

ClickHouse的支持目前存在问题， 详见：https://github.com/denis-ivanov/EntityFrameworkCore.ClickHouse/issues/17
