# IoTSharp.Extensions.QuartzJobScheduler

使用 QuartzJobScheduler 建议的方式，实现 IoTSharp 的定时任务调度。不用再写一堆任务注册和配置的代码了。只需要在实现了IJob的类上加上一个特性QuartzJobScheduler，就可以自动注册到QuartzJobScheduler中。

