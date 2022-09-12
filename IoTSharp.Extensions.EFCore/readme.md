## What is IoTSharp.Extensions.EFCore?

[![Nuget Version](https://img.shields.io/nuget/v/IoTSharp.Extensions.EFCore.svg)](https://www.nuget.org/packages/IoTSharp.Extensions.EFCore/)

 IoTSharp.Extensions.EFCore is an extension for EF.Core, and the main features include executing the original sql statement, converting the original sql statement to a tuple or a class or array or json  object or DataTable .

IoTSharp.Extensions.EFCore 是一个针对EF.Core的扩展， 主要功能包括 执行原始sql语句， 将原始sql语句转换为 元组或者类或者数组。 

 ##  示例:

```c#
_context.Database.SqlQuery<ImgInfo>("select * from ImageInfo  litmt 1 ").FirstOrDefault();

_context.Database.SqlQuery<(int id,string body)>("select id,body from imageInfo").ToListAsync();

 _context.Database.ExecuteScalar<long>($"select  count(0)  from pg_class where relname = '{ImageInfo}'");

​     var r = _context.Database.ExecuteNonQuery(Properties.Resources._imageinfo);

 _context.Database.ExecuteScalar<long>("select  count(0)  from pg_class where relname = {0}","asdfds");
```

