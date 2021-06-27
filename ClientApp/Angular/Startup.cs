using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProxyKit;

namespace IoTSharp.ClientApp.ng_alain
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddControllersWithViews();
      // In production, the Angular files will be served from this directory
      services.AddSpaStaticFiles(configuration =>
      {
        configuration.RootPath = "wwwroot";
      });
      services.AddProxy();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseStaticFiles();
      if (!env.IsDevelopment())
      {
        app.UseSpaStaticFiles();
      }

      app.UseRouting();
      app.Map("/api", appProxy =>
            {
              appProxy.RunProxy(p => p.ForwardTo($"{Configuration["upstream-server"]}/api")
            .AddXForwardedHeaders()
            .Send());
            });
      app.Map("/swagger", appProxy =>
      {
        appProxy.RunProxy(p => p.ForwardTo($"{Configuration["upstream-server"]}/swagger")
      .AddXForwardedHeaders()
      .Send());
      });
      app.Map("/healthchecks-ui", appProxy =>
      {
        appProxy.RunProxy(p => p.ForwardTo($"{Configuration["upstream-server"]}/healthchecks-ui")
      .AddXForwardedHeaders()
      .Send());
      });
      app.Map("/quartzmin", appProxy =>
      {
        appProxy.RunProxy(p => p.ForwardTo($"{Configuration["upstream-server"]}/quartzmin")
      .AddXForwardedHeaders()
      .Send());
      });
      app.Map("/cap", appProxy =>
      {
        appProxy.RunProxy(p => p.ForwardTo($"{Configuration["upstream-server"]}/cap")
      .AddXForwardedHeaders()
      .Send());
      });

      app.UseEndpoints(endpoints =>
{
});

      app.UseSpa(spa =>
      {
        // To learn more about options for serving an Angular SPA from ASP.NET Core,
        // see https://go.microsoft.com/fwlink/?linkid=864501

        spa.Options.SourcePath = "./";

        if (env.IsDevelopment())
        {
          spa.UseAngularCliServer(npmScript: "start");
        }
      });
    }
  }
}
