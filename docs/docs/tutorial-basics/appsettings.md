---
sidebar_position: 5
---

# 配置IoTSharp

​		本教程主要讲述 appsettings 的配置 , 大家可以在 代码目录或者安装文件目录看到  有很多个 appsettings.xxxxx.json 的文件, 主要的默认配置， 我们是通过 appsettings.json  配置。但是由于开发需要， 我们提供了根据数据库不通而不通的配置， 可以根据你使用情况来参考这些配置。 比如， 环境中使用Mysql ， 可以把   appsettings.MySql.json  改为 appsettings.Production.json  。但推荐通过环境变量 ASPNETCORE_ENVIRONMENT  来决定使用的配置文件，   比如 ASPNETCORE_ENVIRONMENT 设置为 MySQL ， 使用的配置文件就是 appsettings.MySQL.json 文件， 如果ASPNETCORE_ENVIRONMENT 设置为 Sqlite，使用的配置文件就是 appsettings.Sqlite.json 文件。于此同时， 在VS中调试时 ， 也通过 launchSettings.json 文件预配了支持的数据库 环境变量和对应的文件 ， 方便调试， 只需要在VS中选择调试环境即可。  

# 数据库和中间件配置

  开始使用前， 我们需要最先了解的应该是数据库， 数据如何存放， 时序数据如何存放等， 这里我们考虑到了各种情况， 多种数据库和多种中间件的组合，你可以根据你的喜好， 选择五种关系型数据库的其中一个， 也可以从我们支持的四个时序数据库中选择一个， 当然， 你可以选择在关系数据库中存储时序数据， 可以选择单表 ， 也可以选择分表， 如果分表， 你可以选择按分钟， 按日， 按月， 按年，也可以选择各种支持的 消息中间件等，  下面我们描述如何配置他们：

1. 你需要通过 "DataBase" 来指定关系型数据库， 比如指定为  "Sqlite"。 
2. 配置关系型数据库连接字符串 ， 通过  "ConnectionStrings"  中的 "IoTSharp"配置项配置关系型数据库连接字符串，  比如 "Data Source=IoTSharp.db"
3. 配置时序存储模式 ， 通过 "TelemetryStorage" 来配置时序数据存储方式， 比如 我们在Sqlite中使用分表模式， 那么就需要 "Sharding" , 如果是单表就填写为 SingleTable , 如果使用InfluxDB , 则连写InfluxDB
4. 配置时序存储连接字符串 ，通过  "ConnectionStrings"中的  "TelemetryStorage"我们配置使用Sqlite的分表模式， 那么连接字符串就是这样， “Data Source=TelemetryStorage.db”，  
5. 配置事件总线中间件, 通过配置项"EventBusMQ" 来配置消息总线使用什么中间件， 你可以配置 RabbitMQ或 内存模式 InMemory ，如果使用了InMemory可以不用配置连接字符串，请忽略 第六条。  
6. 配置事件总线连接字符串 ，通过"ConnectionStrings"中的 "EventBusMQ" 来配置连接字符串， 内存模式时不需要配人， 但比如当我们使用 RabbitMQ等中间件时则需要配置， 比如  "EventBusMQ": "amqp://root:kissme@rabbitmq:5672"
7. 配置事件总线消息存储方式， 我们通过 "EventBusStore" 来设置用何种方式来存储消息， 比如使用， MongoDB, 那么就需要将配置项 "EventBusStore" 改为 MongoDB, 也可以使用 InMemory， 如果使用了InMemory可以不配置 连接字符串，请忽略第八条。 
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

 1. SingleTable
 2. Sharding
 3. Taos
 4. InfluxDB
 5. PinusDB
 6. TimescaleDB
 7. IoTDB

# 支持的事件总线
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

