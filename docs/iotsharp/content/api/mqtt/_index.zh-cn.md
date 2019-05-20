# MQTT API说明

## MQTT 验证:

目前支持以下验证方式:

AccessToken   每个设备创建时自动生成一个 AccessToken，MQTT连接时 ， 用户名填写 AccessToken 值即可。 

DevicePassword  用户名密码方式登录， 在连接时填写指定的用户名密码即可。 

X509Certificate   X509 加密方式， 此方式正在测试和验证。



## Topic 规则介绍

### 设备:

属性数据   /devices/me/attributes

遥测数据  /devices/me/telemetry



### 网关:

上传自身属性数据   /devices/me/attributes

上传自身遥测数据  /devices/me/telemetry

上传子设备属性数据   /devices/<子设备名称>/attributes

上传子设备遥测数据  /devices/<子设备名称>/telemetry

上传子设备的属性和遥测数据时 ， 首先会查找设备是否存在， 如果存在， 则创建设备， 创建设备的租户和客户信息继承自网关，自动创建的设备与普通设备没有区别。

子设备名称如果指定的是已存在设备或者其他网关的子设备则不能创建， 也就是说， 一个客户的所有设备名称是唯一的。 



## 数据类型

### XML 类型 :

主题规则:  /devices/me/attributes/xml/<KeyName>

KeyName 是 键值名称，Playload  内容为值，存入钱程序会使用xlm加载器尝试加载一遍是否是正确的xml，如果有任何异常， 则不保存，如果格式无误能正常加载，则保存。 

#### 二进制类型:

主题规则:  /devices/me/attributes/binary/<KeyName>

KeyName 是 键值名称， Playload 内容为二进制数组。 

#### 常规类型

常规数据类型完全通过你发送的Playload 中的Json格式来决定

单独KeyValue 方式: 

  {"KeyName":"KeyValue"}

数组KeyValue 方式:

[

{"KeyName": "KeyValue" },

{"IntKeyName":  32 },

{"JsonKeyName":  {

​	"Name":"maike.ma"

​	,"Age":35 

​      }

}

]





## 数据存储

 

当有新数据进来后， 如果键值已经存在， 则修改键值对应的值 ， 如果键值不存在， 则创建，并像历史数据中新增一条。

目前我们将所有数据都存在PostgreSQL , 如果数据量巨大， 可以根据情况存放在  *MongoDB*   、*Cassandra*      等 NoSQL 数据库中。 