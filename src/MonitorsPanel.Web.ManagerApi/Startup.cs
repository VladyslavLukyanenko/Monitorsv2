using System.Net.Mime;
using System.Threading.Tasks;
using Amazon;
using Amazon.EC2;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MonitorsPanel.Core.Manager;
using MonitorsPanel.Core.Manager.Primitives;
using MonitorsPanel.Core.Manager.Services;
using MonitorsPanel.Core.Manager.Services.Aws;
using MonitorsPanel.Core.Manager.Services.GoogleComputeEngine;
using MonitorsPanel.Web.ManagerApi.App.Model;
using MonitorsPanel.Web.ManagerApi.App.Services;
using MonitorsPanel.Web.ManagerApi.App.Web;
using MonitorsPanel.Web.ManagerApi.Foundation;
using MonitorsPanel.Web.ManagerApi.Foundation.Config;
using MonitorsPanel.Web.ManagerApi.Foundation.SwaggerSupport.Swashbuckle;
using MonitorsPanel.Web.ManagerApi.Infra;
using MonitorsPanel.Web.ManagerApi.Infra.Consumers;
using MonitorsPanel.Web.ManagerApi.Infra.RabbitMQ;
using MonitorsPanel.Web.ManagerApi.Infra.Repositories;

namespace MonitorsPanel.Web.ManagerApi
{
  public class Startup
  {
    private const string ApiVersion = "v1";
    private const string ApiTitle = "Monitors Panel API";
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddHostedService<ServerInstancesStatsHostedService>();

      services.AddScoped<IImageInfoRepository, EfImageInfoRepository>()
        .AddScoped<IImagesManager, ImagesManager>()
        .AddScoped<IInfrastructureClient, AwsInfrastructureClient>()
        // .AddScoped<IInfrastructureClient, GoogleComputeEngineInfrastructureClient>()
        .AddScoped<IDockerManager, DockerManager>()
        .AddScoped<IDockerAuthProvider, LoginPwdDockerAuthProvider>()
        .AddScoped<IImagesRuntimeInfoService, ImagesRuntimeInfoService>()
        .AddScoped<IUnitOfWork, DbContextUnitOfWork>();

      services.AddSingleton<IDockerClientsProvider, DockerClientsProvider>()
        .AddSingleton<IComponentsStateRegistry, InMemoryComponentsStateRegistry>();

      services.AddSingleton(c =>
      {
        var config = c.GetService<AwsConfig>();
        return new AmazonEC2Client(config.AccessKeyId, config.SecretAccessKey,
          RegionEndpoint.GetBySystemName(config.PlacementRegion));
      });
      // services.AddSingleton(c =>
      // {
      //   var config = c.GetService<GoogleComputeEngineConfig>();
      //   return new ComputeService(new BaseClientService.Initializer
      //   {
      //     ApplicationName = config.ApplicationName,
      //     HttpClientInitializer = GetCredential(config)
      //   });
      // });

      services.AddMassTransit(_ =>
        {
          _.AddConsumer<ComponentStatsConsumer>();
          _.SetSnakeCaseEndpointNameFormatter();
          _.UsingRabbitMq((ctx, cfg) =>
          {
            var connCfg = ctx.GetService<DataSourceConfig>();
            cfg.Host(connCfg.RabbitMq.Host, connCfg.RabbitMq.Port, "/", q =>
            {
              q.Username(connCfg.RabbitMq.Username);
              q.Password(connCfg.RabbitMq.Password);
            });
            cfg.AddMessageDeserializer(new ContentType("application/json"),
              () => new ApplicationJsonMessageDeserializer(typeof(ComponentStatsEntry)));
            cfg.ConfigureEndpoints(ctx);
          });
        })
        .AddMassTransitHostedService();

      services
        .InitializeConfiguration(_configuration)
        .AddApplicationDbContext(_configuration)
        .AddConfiguredCors(_configuration)
        .AddConfiguredMvc()
        .AddConfiguredSignalR()
        .AddConfiguredAuthentication(_configuration)
        .AddConfiguredSwagger(ApiVersion, ApiTitle);


      services.AddIdentityServer(options => { options.EmitStaticAudienceClaim = true; })
        .AddDeveloperSigningCredential()
        .AddInMemoryClients(IdentityServerStaticConfig.GetClients())
        .AddInMemoryApiResources(IdentityServerStaticConfig.GetApiResources())
        .AddInMemoryIdentityResources(IdentityServerStaticConfig.GetIdentityResources())
        .AddTestUsers(IdentityServerStaticConfig.GetUsers());
      services.AddIdentityServerConfiguredCors(_configuration);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      var corsConfig = app.UseCommonHttpBehavior(env);
      app.UseStaticFiles();
      app.UseIdentityServer();
      app.UseRouting();
      app.UseConfiguredCors(corsConfig);

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
        endpoints.MapDefaultControllerRoute();
      });

      app.UseConfiguredSwagger(ApiVersion, ApiTitle);
    }

    private static IConfigurableHttpClientInitializer GetCredential(GoogleComputeEngineConfig config)
    {
      GoogleCredential credential = Task.Run(() => GoogleCredential.FromFile(config.CredentialsPath)).Result;
      if (credential.IsCreateScopedRequired)
      {
        credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
      }

      return credential;
    }
  }
}