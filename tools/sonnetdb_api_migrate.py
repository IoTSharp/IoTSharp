#!/usr/bin/env python
from __future__ import print_function

import argparse
import json
import sys
import time

try:
    import urllib2
except ImportError:
    import urllib.error
    import urllib.request

    class urllib2(object):
        Request = urllib.request.Request
        HTTPError = urllib.error.HTTPError

        @staticmethod
        def urlopen(request, timeout=None):
            return urllib.request.urlopen(request, timeout=timeout)

PY2 = sys.version_info[0] == 2
if not PY2:
    unicode = str
    basestring = (str, bytes)
    long = int


DEFAULT_TOKEN = "iotsharp_sonnetdb_dev_token_change_me"
DEFAULT_SOURCE = "http://127.0.0.1:35080/v1/db/iotsharp/sql"
DEFAULT_TARGET = "http://127.0.0.1:25080/v1/db/iotsharp/sql"

TABLE_RENAMES = {
    "Produces": "Products",
    "ProduceDictionaries": "ProductDictionaries",
}

COLUMN_RENAMES = {
    ("Produces", "Products"): {
        "ProduceToken": "ProductToken",
    },
    ("ProduceDictionaries", "ProductDictionaries"): {
        "ProduceId": "ProductId",
    },
}

SKIP_TABLES = set([
    "__EFMigrationsHistory",
    "DataStorage",
    "TelemetryData",
    "Device",
    "DeviceIdentities",
    "ProduceDataMappings",
    "DeviceDiagrams",
    "DeviceGraphs",
    "DeviceGraphToolBoxes",
    "DeviceModelCommands",
    "DeviceModels",
    "DevicePortMappings",
    "DevicePorts",
    "DeviceRules",
    "SubscriptionEvents",
    "SubscriptionTasks",
])

PREFERRED_ORDER = [
    "AspNetRoles",
    "AspNetUsers",
    "AspNetRoleClaims",
    "AspNetUserClaims",
    "AspNetUserLogins",
    "AspNetUserTokens",
    "AspNetUserRoles",
    "Tenant",
    "Customer",
    "Relationship",
    "BaseDictionaryGroups",
    "BaseDictionaries",
    "Produces",
    "ProduceDictionaries",
    "Assets",
    "AssetRelations",
    "AISettings",
    "RefreshTokens",
    "Alarms",
    "BaseEvents",
    "AuditLog",
    "DynamicFormInfos",
    "DynamicFormFieldInfos",
    "DynamicFormFieldValueInfos",
    "Flows",
    "FlowRules",
    "FlowOperations",
    "RuleTaskExecutors",
    "EdgeNodes",
]


def fail(message):
    print("ERROR: %s" % message, file=sys.stderr)
    sys.exit(1)


def post_sql(url, token, sql, timeout):
    body = json.dumps({"sql": sql})
    if isinstance(body, unicode):
        body = body.encode("utf-8")
    request = urllib2.Request(url, data=body)
    request.add_header("Authorization", "Bearer %s" % token)
    request.add_header("Content-Type", "application/json")
    try:
        response = urllib2.urlopen(request, timeout=timeout)
        raw = response.read()
        status = response.getcode()
    except urllib2.HTTPError as ex:
        raw = ex.read()
        status = ex.code
    except Exception as ex:
        raise RuntimeError("request failed for SQL [%s]: %s" % (sql, ex))

    if status < 200 or status >= 300:
        raise RuntimeError("HTTP %s for SQL [%s]: %s" % (status, sql, raw))

    rows = []
    columns = None
    end = None
    for line in raw.splitlines():
        if not line.strip():
            continue
        item = json.loads(line)
        if isinstance(item, dict) and item.get("type") == "meta":
            columns = item.get("columns") or []
        elif isinstance(item, dict) and item.get("type") == "end":
            end = item
        elif isinstance(item, dict) and item.get("error"):
            raise RuntimeError("SQL error for [%s]: %s" % (sql, item))
        else:
            rows.append(item)
    return columns, rows, end


def scalar_count(url, token, table, timeout):
    columns, rows, end = post_sql(url, token, "SELECT COUNT(*) AS Count FROM %s" % quote_ident(table), timeout)
    if not rows:
        return 0
    return int(rows[0][0])


def list_tables(url, token, timeout):
    columns, rows, end = post_sql(url, token, "SHOW TABLES", timeout)
    return [row[0] for row in rows]


def describe_table(url, token, table, timeout):
    columns, rows, end = post_sql(url, token, "DESCRIBE TABLE %s" % quote_ident(table), timeout)
    name_index = 0
    type_index = None
    for i, name in enumerate(columns or []):
        lower = name.lower()
        if lower in ("name", "column_name"):
            name_index = i
        if lower in ("data_type", "type"):
            type_index = i
    result = []
    for row in rows:
        item = {"name": row[name_index]}
        if type_index is not None and type_index < len(row):
            item["type"] = row[type_index]
        result.append(item)
    return result


def quote_ident(name):
    if PY2 and isinstance(name, str):
        name = name.decode("utf-8")
    return u'"' + name.replace(u'"', u'""') + u'"'


def sql_literal(value):
    if value is None:
        return "NULL"
    if isinstance(value, bool):
        return "TRUE" if value else "FALSE"
    if isinstance(value, (int, long, float)):
        return repr(value)
    if isinstance(value, list) or isinstance(value, dict):
        value = json.dumps(value, separators=(",", ":"), ensure_ascii=False)
    if not isinstance(value, basestring):
        value = str(value)
    if PY2 and isinstance(value, str):
        value = value.decode("utf-8")
    return u"'" + value.replace(u"'", u"''") + u"'"


def build_insert(table, columns, rows):
    col_sql = ", ".join(quote_ident(c) for c in columns)
    value_sql = []
    for row in rows:
        value_sql.append("(" + ", ".join(sql_literal(v) for v in row) + ")")
    return "INSERT INTO %s (%s) VALUES %s" % (quote_ident(table), col_sql, ", ".join(value_sql))


def ordered_tables(source_tables):
    used = set()
    result = []
    for table in PREFERRED_ORDER:
        if table in source_tables and table not in used:
            result.append(table)
            used.add(table)
    for table in sorted(source_tables):
        if table not in used:
            result.append(table)
            used.add(table)
    return result


def fetch_rows(url, token, table, columns, count, timeout):
    if count == 0:
        return []
    select_cols = ", ".join(quote_ident(c) for c in columns)
    sql = "SELECT %s FROM %s LIMIT %d" % (select_cols, quote_ident(table), count)
    got_columns, rows, end = post_sql(url, token, sql, timeout)
    return rows


def plan(args):
    source_tables = set(list_tables(args.source_url, args.token, args.timeout))
    target_tables = set(list_tables(args.target_url, args.token, args.timeout))
    planned = []
    skipped = []

    for source_table in ordered_tables(source_tables):
        target_table = TABLE_RENAMES.get(source_table, source_table)
        if source_table in SKIP_TABLES:
            skipped.append((source_table, target_table, "skip_policy"))
            continue
        if target_table not in target_tables:
            skipped.append((source_table, target_table, "target_missing"))
            continue

        count = scalar_count(args.source_url, args.token, source_table, args.timeout)
        if count == 0:
            skipped.append((source_table, target_table, "empty"))
            continue

        source_schema = describe_table(args.source_url, args.token, source_table, args.timeout)
        target_schema = describe_table(args.target_url, args.token, target_table, args.timeout)
        target_cols = set(c["name"] for c in target_schema)
        column_renames = COLUMN_RENAMES.get((source_table, target_table), {})
        source_columns = []
        target_columns = []
        for column in source_schema:
            source_column = column["name"]
            target_column = column_renames.get(source_column, source_column)
            if target_column in target_cols:
                source_columns.append(source_column)
                target_columns.append(target_column)

        if not source_columns:
            skipped.append((source_table, target_table, "no_common_columns"))
            continue

        target_count = scalar_count(args.target_url, args.token, target_table, args.timeout)
        planned.append({
            "source": source_table,
            "target": target_table,
            "count": count,
            "target_count": target_count,
            "source_columns": source_columns,
            "target_columns": target_columns,
        })

    return planned, skipped


def migrate(args, planned):
    total = 0
    for item in planned:
        source = item["source"]
        target = item["target"]
        count = item["count"]
        source_columns = item["source_columns"]
        target_columns = item["target_columns"]
        if item["target_count"] != 0 and not args.force_nonempty:
            fail("target table %s is not empty (%d rows); use --force-nonempty to append" % (target, item["target_count"]))
        print("MIGRATE %s -> %s rows=%d columns=%d" % (source, target, count, len(source_columns)))
        rows = fetch_rows(args.source_url, args.token, source, source_columns, count, args.timeout)
        if len(rows) != count:
            fail("table %s expected %d rows, got %d" % (source, count, len(rows)))
        for start in range(0, len(rows), args.batch_size):
            batch = rows[start:start + args.batch_size]
            sql = build_insert(target, target_columns, batch)
            cols, result_rows, end = post_sql(args.target_url, args.token, sql, args.timeout)
            affected = int((end or {}).get("recordsAffected", 0))
            if affected != len(batch):
                fail("table %s batch affected %d, expected %d" % (target, affected, len(batch)))
            total += affected
        print("DONE %s -> %s inserted=%d" % (source, target, count))
    return total


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--source-url", default=DEFAULT_SOURCE)
    parser.add_argument("--target-url", default=DEFAULT_TARGET)
    parser.add_argument("--token", default=DEFAULT_TOKEN)
    parser.add_argument("--timeout", type=int, default=15)
    parser.add_argument("--batch-size", type=int, default=50)
    parser.add_argument("--migrate", action="store_true")
    parser.add_argument("--force-nonempty", action="store_true")
    args = parser.parse_args()

    started = time.time()
    planned, skipped = plan(args)
    print("PLAN")
    for item in planned:
        print("%s -> %s source=%d target=%d columns=%s" % (
            item["source"],
            item["target"],
            item["count"],
            item["target_count"],
            ",".join([
                "%s=%s" % pair if pair[0] != pair[1] else pair[0]
                for pair in zip(item["source_columns"], item["target_columns"])
            ])))
    print("SKIPPED")
    for source, target, reason in skipped:
        print("%s -> %s reason=%s" % (source, target, reason))

    if args.migrate:
        inserted = migrate(args, planned)
        print("INSERTED %d" % inserted)
    else:
        print("DRY_RUN")
    print("ELAPSED %.2fs" % (time.time() - started))


if __name__ == "__main__":
    main()
