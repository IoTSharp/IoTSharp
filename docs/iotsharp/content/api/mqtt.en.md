# MQTT API Description

## MQTT Validation:

The following authentication methods are currently supported:

AccessToken when a ACCESSTOKEN,MQTT connection is automatically generated when each device is created, the user name fills in the AccessToken value. 

DevicePassword User name password login, in the connection to fill in the specified user name password can be. 

X509Certificate X509 Encryption method, this method is being tested and validated.



## Introduction to Topic Rules

### Devices:

Attribute data   /devices/me/attributes

Telemetry data  /devices/me/telemetry



### Gateway:

Upload attribute data for the device itself   /devices/me/attributes

Upload telemetry data for the device itself  /devices/me/telemetry

Upload Child device attribute data   /devices/<Child device Name>/attributes

Upload Child device Telemetry data  /devices/<Child device Name>/telemetry

When uploading the properties and telemetry data of a child device, you first find out if the device exists and, if so, create a device, create a device's tenant and customer information inherited from the gateway, and automatically create a device that is no different from a normal device. Child device Name If you specify a child device that already has a device or other gateway, you cannot create it, that is, all device names for a customer are unique.



## Data Type

### XML :

Topic rules:  /devices/me/attributes/xml/<KeyName>

KeyName is the key name, Playload content is a value, the pre-deposit program will use the XLM loader to try to load whether the correct XML, if there are any exceptions, it is not saved, if the format is correct to load properly, then save. 

#### Binary:

主题规则:  /devices/me/attributes/binary/<KeyName>

KeyName is the key name, and the Playload content is a binary array. 

#### General type

The general data type is completely determined by the Json format in the Playload you send

Single KeyValue mode: 

  {"KeyName":"KeyValue"}

Array KeyValue mode:

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