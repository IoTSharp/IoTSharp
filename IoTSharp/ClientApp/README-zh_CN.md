如何开发

确保安装Angular 11.2.14
npm i @angular/cli@11.2.14
目前使用Nginx跨域，以下是Nginx配置文件，在nginx的\conf\nginx.conf中增加即可
 server{
        listen 8888; 
        server_name  localhost; #实际访问的地址，localhost:8888
        location /{
            proxy_pass http://localhost:4200; #前端地址，angualr 默认4200 根据实际情况填写

        }
        location /api{
            proxy_pass http://localhost:46931; #APi地址，根据实际情况填写
        }
        location /Attachments{
            proxy_pass http://localhost:46931; #APi地址，附件上传的地址
        }  
    }


VS中启动Web项目，然后启动前端，在Angular项目根目录执行
npm install
ng serve

启动nginx,访问localhost:8888

如何部署

安装 Microsoft.AspNetCore.SpaServices.Extensions，
在Startup.cs 的ConfigureServices(IServiceCollection services) 方法增加
    services.AddSpaStaticFiles(configuration =>
            {
              configuration.RootPath = "ClientApp/dist";
            });
在Configure(IApplicationBuilder app, IWebHostEnvironment env)方法增加
    app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
 把Angular文件夹下的文件拷贝到Web项目的根目录下的ClientApp目录当中，没有 ClientApp目录，则新建一个。          
最后启动项目，确保正常启动，然后像发布普通Web项目一样，点击生成，发布即可






VS可以直接创建Angular模板项目，只是VS构建实在太慢了，2022有改善，速度比分离开发还是慢。