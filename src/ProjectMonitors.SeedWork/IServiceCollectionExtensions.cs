using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Elastic.Apm;
using Elastic.Apm.Api;
using Elastic.Apm.Config;
using Elastic.Apm.DiagnosticSource;
using Elastic.Apm.Logging;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ProjectMonitors.SeedWork.Config;
using ProjectMonitors.SeedWork.Domain;
using ProjectMonitors.SeedWork.Infra;
using RabbitMQ.Client;

namespace ProjectMonitors.SeedWork
{
  // ReSharper disable once InconsistentNaming
  public static class IServiceCollectionExtensions
  {
    public static IServiceCollection AddConfiguredRabbitMq(this IServiceCollection services,
      Action<IServiceProvider, IModel>? channelConfigurer = null) =>
      services.AddSingleton(ctx =>
        {
          var cfg = ctx.GetRequiredService<IConfiguration>();
          var activitySource = ctx.GetRequiredService<ActivitySource>();
          using var _ = activitySource.StartActivity("connect_rmq");
          var connFactory = new ConnectionFactory
          {
            Uri = new Uri(cfg.GetConnectionString("RabbitMq")),
            DispatchConsumersAsync = true
          };

          return connFactory.CreateConnection();
        })
        .AddSingleton(ctx =>
        {
          var conn = ctx.GetRequiredService<IConnection>();
          var activitySource = ctx.GetRequiredService<ActivitySource>();
          using var _ = activitySource.StartActivity("configure_rmq");
          var model = conn.CreateModel();
          model.ExchangeDeclare(RmqRoutes.ComponentExchangeName, "fanout", true);
          model.ExchangeDeclare(RmqRoutes.BalancerPublishExchangeName, "direct", true);
          model.ExchangeDeclare(RmqRoutes.SenderPublishExchangeName, "direct", true);
          model.ExchangeDeclare(RmqRoutes.BalancerConnectExchangeName, "direct", true);

          channelConfigurer?.Invoke(ctx, model);

          var balancerQueue = model.QueueDeclare(RmqRoutes.BalancerPublishQueueName, true, false, false);
          model.QueueBind(balancerQueue.QueueName, RmqRoutes.BalancerPublishExchangeName,
            RmqRoutes.BalancerPublishRoutingKey);

          var connectQueue = model.QueueDeclare(RmqRoutes.BalancerConnectQueueName, true, false, false);
          model.QueueBind(connectQueue.QueueName, RmqRoutes.BalancerConnectExchangeName,
            RmqRoutes.BalancerConnectRoutingKey);

          model.BasicQos(0, 1, false);

          return model;
        });

    public static IServiceCollection AddConfiguredCoreServices(this IServiceCollection services) =>
      services.AddSingleton<IJsonSerializer, SystemTextJsonSerializer>()
        .AddSingleton<IBinarySerializer, MessagePackBinarySerializer>()
        .AddSingleton(_ => new JsonSerializerOptions
        {
          PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        })
        .AddSingleton(_ =>
          MessagePackSerializerOptions.Standard
            .WithResolver(CompositeResolver.Create(
              StandardResolver.Instance,
              ContractlessStandardResolver.Instance,
              StandardResolverAllowPrivate.Instance,
              DynamicEnumAsStringResolver.Instance,
              TypelessObjectResolver.Instance
            )));

    public static IServiceCollection AddConfiguredTelemetry(this IServiceCollection services) =>
      services
        .AddSingleton(_ => new ActivitySource(AppConstants.AppName, AppConstants.InformationalVersion))
        .AddSingleton(ctx =>
        {
          var env = ctx.GetRequiredService<IHostEnvironment>();
          var cfg = ctx.GetRequiredService<IConfiguration>();
          var activitySource = ctx.GetRequiredService<ActivitySource>();
          return Sdk.CreateTracerProviderBuilder()
            // .SetSampler(new AlwaysOnSampler())
            .AddSource(activitySource.Name)
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
              .AddTelemetrySdk()
              .AddAttributes(new Dictionary<string, object>
              {
                {"deployment.environment", env.EnvironmentName},
                // {"transaction.type", "app"},
              })
              .AddService(AppConstants.AppName, serviceVersion: AppConstants.InformationalVersion)
              .AddEnvironmentVariableDetector())
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(c => { c.Endpoint = new Uri(cfg["Otlp:PublisherUrl"]); })
            .Build();
        })
        .AddSingleton(s =>
        {
          return s.GetRequiredService(typeof(ILoggerFactory)) is ILoggerFactory loggerFactory
            ? (IApmLogger) ReflectionHelper.CreateInstanceOfInternalClass("Elastic.Apm.Extensions.Hosting",
              "Elastic.Apm.Extensions.Hosting", "NetCoreLogger", new object[] {loggerFactory})
            : (IApmLogger) ReflectionHelper.GetStaticInternalProperty("Elastic.Apm", "Elastic.Apm.Logging",
              "ConsoleLogger", "Instance");
        })
        .AddSingleton(s =>
        {
          var env = s.GetRequiredService<IHostEnvironment>();
          var config = s.GetRequiredService<IConfiguration>();

          var configAdapter = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
              {"ElasticApm:ServiceName", AppConstants.AppName}
            })
            .AddConfiguration(config);

          var apmLogger = s.GetRequiredService<IApmLogger>();
          var configurationReader = ReflectionHelper.CreateInstanceOfInternalClass("Elastic.Apm.Extensions.Hosting",
            "Elastic.Apm.Extensions.Hosting.Config", "MicrosoftExtensionsConfig", new object[]
            {
              configAdapter.Build(),
              apmLogger,
              env.EnvironmentName
            }) as IConfigurationReader;
          
          return configurationReader;

        })
        .AddSingleton(s =>
        {
          var components = new AgentComponents(s.GetRequiredService<IApmLogger>(), s.GetRequiredService<IConfigurationReader>());
          components.Service.Framework = new Framework
            {Name = ".NET Core", Version = Environment.Version.ToString(3)};
          components.Service.Language = new Language {Name = "C#"};
          return components;
        })
        .AddSingleton<IApmAgent>(s =>
        {
          var agentConfig = s.GetRequiredService<AgentComponents>();
          var subscribers = s.GetRequiredService<IEnumerable<IDiagnosticsSubscriber>>();
          var agent = (IApmAgent) ReflectionHelper.CreateInstanceOfInternalClass("Elastic.Apm", "Elastic.Apm",
            "ApmAgent", new object[] {agentConfig});
          agent.Subscribe(subscribers.ToArray());
          return agent;

        })
        .AddSingleton<IDiagnosticsSubscriber>(_ => new HttpDiagnosticsSubscriber())
        .AddSingleton<ITracer>(s => s.GetRequiredService<IApmAgent>().Tracer);
  }
}