---
sidebar_position: 5
---

# 如何配置IoTSharp?

    本教程主要讲述 appsettings 的配置 , 大家可以在 代码目录或者安装文件目录看到
  有很多个 appsettings.xxxxx.json 的文件, 主要的默认配置， 我们是通过 appsettings.json
  配置。但是由于开发需要， 我们提供了根据数据库不通而不通的配置， 你可以
  根据你自己的时机情况来参考这些配置。 比如， 你环境中使用Mysql ， 那么你可以把 
  appsettings.mysql.json  改为 appsettings.Production.json , 或者我们推荐另外一种方式。 
  你可以不用重命名文件， 而是使用环境变量 ASPNETCORE_ENVIRONMENT ， 为 此环境变量赋值  ， 
  比如 ASPNETCORE_ENVIRONMENT 设置为 MySQL ， 那么使用的配置文件就是 appsettings.mysql.json 文件， 
  如果appsettings.mysql.json设置为 Sqlite，那么使用的配置文件就是 appsettings.sqlite.json 文件。 
  
# 数据库配置项

  下面我们描述如何配置数据库
  
  1. 你需要通过 "DataBase" 来指定关系型数据库， 比如指定为  "Sqlite", 具体可查看 【支持的关系型数据库配置项】
  2. 配置关系型数据库连接字符串 ， 通过  "ConnectionStrings"  中的 "IoTSharp"配置项配置关系型数据库连接字符串，  比如 "Data Source=IoTSharp.db"
  3. 配置时序存储模式 ， 通过 "TelemetryStorage" 来配置时序数据存储方式， 比如 我们在Sqlite中使用分表模式， 那么就需要 "Sharding" , 如果是单表就填写为 SingleTable , 如果使用InfluxDB , 则连写InfluxDB
  4. 配置时序存储连接字符串 ，通过  "ConnectionStrings"中的  "TelemetryStorage"我们配置使用Sqlite的分表模式， 那么连接字符串就是这样， “Data Source=TelemetryStorage.db”，  
  5. 配置事件总线中间件, 通过配置项"EventBusMQ" 来配置消息总线使用什么中间件， 你可以配置 RabbitMQ或 内存模式 InMemory ， 
  6. 配置事件总线连接字符串 ，通过"ConnectionStrings"中的 "EventBusMQ" 来配置连接字符串， 内存模式时不需要配人， 但比如当我们使用 RabbitMQ等中间件时则需要配置， 比如  "EventBusMQ": "amqp://root:kissme@rabbitmq:5672"
  7. 配置事件总线消息存储方式， 我们通过 "EventBusStore" 来设置用何种方式来存储消息， 比如使用， MongoDB, 那么就需要将配置项 "EventBusStore" 改为 MongoDB, 也可以使用 InMemory
  8. 配置事件消息存储连接字符串 ， 如果使用了MongoDB 等一些存储消息的组件， 那么需要通过 通过"ConnectionStrings"中的 "EventBusStore" 来配置， 比如 如果使用了MongoDB 的连接字符串是 "mongodb://root:kissme@mongodb:27017

# 支持的关系型数据库配置项
配置项名称是 DataBase

  1. PostgreSql
  2. SqlServer
  3. MySql
  4. Oracle
  5. Sqlite
  6. InMemory
  7. Cassandra
  
 # 支持的时序数据库及其配置项
 配置项名称是 TelemetryStorage 
 
 1.  SingleTable
 2.  Sharding
 3.  Taos
 4. InfluxDB
 5. PinusDB
 6. TimescaleDB
 7. IoTDB

# 支持的时间总线
 配置项名称为 EventBusMQ
 
 1. RabbitMQ
 2. Kafka
 3. InMemory
 4. ZeroMQ
 5. NATS
 6. Pulsar
 7. RedisStreams
 8. AmazonSQS
 9. AzureServiceBus

# 支持的事件总线存储方式
配置项名称为 EventBusStore
     
   1. PostgreSql
   2. MongoDB  
   3. InMemory
   4. LiteDB
   5. MySql
   6. SqlServer

    
    下面是几个示例:
    

