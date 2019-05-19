# MQTT API说明

## Topic 规则

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

## 

