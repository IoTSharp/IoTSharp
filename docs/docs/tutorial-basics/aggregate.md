---
sidebar_position: 8
---

# 遥测数据聚合

对于物联网平台来说， 聚合是非常重要的数据查询方式， IoTSharp支持指定时间段、遥测、聚合断面、聚合方式等， IoTSharp提供了统一的接口 /api/Devices/id/TelemetryData 来完成这一点。 

参数示例如下:

```json

 { "keys": "", "begin": "2022-03-23T11:44:56.488Z", "every": "1.03:14:56:166", "aggregate": "Mean" }
```
keys  指定了要查询那些遥测， begin 和 end决定了时间范围， end可以忽略， 但是begin必须存在， every 决定了返回的数据与数据之间的时间间隔， 比如每五分钟一条数据， 或者每1秒钟一条数据， aggregate决定了是取最大值？中值 ？或者其他。 


##  支持情况

:::danger 注意
` 目前 InfluxDB 和IoTDB 支持原生的数据聚合，分表和EFCore 关系数据库是通过IoTSharp 本地运算得到的结果， Taos 和 PinusDB 目前不支持 任何形式的聚合，TimescaleDB在后期逐步实现。 
:::