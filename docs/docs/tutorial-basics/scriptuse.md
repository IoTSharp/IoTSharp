# 规则链使用方式

## 一、各种脚本语言执行测试

脚本分类：  javascript脚本、python脚本、sql脚本、lua脚本、csharp脚本

![image.png](/img/iotsharp/script-type.png)

1.javascript脚本

底层是通过第三方库jint来执行javascript脚本代码

https://github.com/sebastienros/jint

脚本参数以参数名：input 传入，所以在方法中取值时需要加上input前缀

如下图所示，写了一个javascript方法，实现计算乘方功能

![image.png](/img/iotsharp/javascript-function.png)

对工作链传参数并执行测试

![image.png](/img/iotsharp/javascript-test.png)

测试结果，可以看到工作链成功接收了参数，并且执行了乘方操作，并反馈结果

![image.png](/img/iotsharp/javascript-result.png)

2.lua脚本

底层是通过NeoLua来执行python语句

https://github.com/neolithos/neolua

写了一个lua的计算乘方函数，接收参数输入并返回乘方结果，输入参数也是以input接收

![image.png](/img/iotsharp/lua-function.png)

传参并测试

![image.png](/img/iotsharp/lua-test.png)

测试结果

![image.png](/img/iotsharp/lua-result.png)

3.sql脚本

此sql非彼sql，不是我们理解的查数据库的sql，这里的sql语法是要符合jsonDB要求的，因为它的数据源是jsonDB封装后的对象

底层用到了两个库：

a. 通过jsonDB库把json对象封装一下，使它支持sql查询

b.通过jint库执行你的sql脚本

jsonDB库地址：https://github.com/thinkeridea/jsonDB

![image.png](/img/iotsharp/sql-code.png)

由于是jsonDB的sql，而且数据源是json对象，并非数据库。 所以这个脚本引擎仅支持简单的crud，函数、存储过程这些都不支持。 

一个简单的例子，过滤姓名="lihong"的人，查出性别

![image.png](/img/iotsharp/sql-function.png)

输入参数

![image.png](/img/iotsharp/sql-test.png)

执行结果

![image.png](/img/iotsharp/sql-result.png)

4.python脚本

底层是通过IronPython执行脚本

https://github.com/IronLanguages/ironpython3

定义乘方函数，注意python需要把返回值复制给output形参，没有这个参数会报错

![image.png](/img/iotsharp/python-function.png)

传参并测试

![image.png](/img/iotsharp/python-test.png)

执行结果

![image.png](/img/iotsharp/python-result.png)

5.csharp脚本

底层通过cs-script动态执行c#脚本代码

c#脚本计算乘方

![image.png](/img/iotsharp/csharp-function.png)

发送数据并测试

![image.png](/img/iotsharp/csharp-test.png)

测试结果

![image.png](/img/iotsharp/csharp-result.png)

## 二、通过mqtt客户端模拟设备告警上报

![image.png](/img/iotsharp/rules-list.png)

设计规则链

![image.png](/img/iotsharp/alarm-rule-design.png)

在javascript脚本中，加入发烧的逻辑判断语句，然后createAlarmDto并返回

![image.png](/img/iotsharp/alarm-rule-function.png)

mqtt客户端连接到平台，并模拟发送数据

![image.png](/img/iotsharp/alarm-rule-mqtt-test.png)

成功创建告警

![image.png](/img/iotsharp/alarm-rule-result.png)