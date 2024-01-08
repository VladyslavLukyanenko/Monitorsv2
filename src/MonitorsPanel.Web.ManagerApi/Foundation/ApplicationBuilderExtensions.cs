using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MonitorsPanel.Web.ManagerApi.Foundation.Config;

namespace MonitorsPanel.Web.ManagerApi.Foundation
{
  public static class ApplicationBuilderExtensions
  {
    public static IApplicationBuilder UseConfiguredSwagger(this IApplicationBuilder app, string apiVersion,
      string apiTitle)
    {
      app.UseSwagger(c => { });
      app.UseSwaggerUI(c => { c.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json", apiTitle); });
      return app;
    }

    public static CorsConfig UseCommonHttpBehavior(this IApplicationBuilder app, IWebHostEnvironment env)
    {
      var startupConfig = app.ApplicationServices.GetRequiredService<IOptions<StartupConfiguration>>().Value;

      if (!env.IsDevelopment())
      {
        app.UseHsts();
      }

      if (startupConfig.UseHttps)
      {
        app.UseHttpsRedirection();
      }

      app.UseForwardedHeaders(new ForwardedHeadersOptions
      {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
      });

      return app.ApplicationServices.GetRequiredService<CorsConfig>();
    }

    public static IApplicationBuilder UseConfiguredCors(this IApplicationBuilder app,
      CorsConfig startupConfig)
    {
      if (startupConfig.UseCors)
      {
        app.UseCors("DefaultCors");
      }

      return app;
    }
  }
}