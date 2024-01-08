using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Mapping;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProjectMonitors.Balancer.Domain;
using ProjectMonitors.Balancer.Infra;
using ProjectMonitors.SeedWork;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Balancer
{
  class Program
  {
    static async Task Main(string[] args)
    {
      await CreateHostBuilder(args)
        .BootstrapWithErrorHandlingAsync(async services =>
        {
          var cache = services.GetRequiredService<SubscriptionsCachingWorker>();
          await cache.FetchSettingsAsync(CancellationToken.None);
        });
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .UseDefaultConsoleAppConfig()
        .ConfigureServices((__, services) =>
        {
          services.AddConfiguredRabbitMq()
            .AddConfiguredCoreServices()
            .AddConfiguredTelemetry()
            .AddSingleton<IMonitorSubscriptionRepository, Linq2DbMonitorSubscriptionRepository>()
            .AddSingleton<IPublisher, RabbitMqPublisher>()
            .AddSingleton<INotificationsRouter, NotificationsRouter>()
            .AddSingleton<ISendersRegistry, InMemorySendersRegistry>()
            .AddSingleton<ISendersProvider>(_ => _.GetRequiredService<ISendersRegistry>())
            .AddSingleton<SubscriptionsCachingWorker>()
            .AddSingleton<ISubscriptionsProvider>(ctx => ctx.GetRequiredService<SubscriptionsCachingWorker>())
            .AddHostedService(ctx => ctx.GetRequiredService<SubscriptionsCachingWorker>())
            .AddHostedService<NotificationsConsumerWorker>()
            .AddSingleton(ctx =>
            {
              var cfg = ctx.GetRequiredService<IConfiguration>();
              var connBuilder = new LinqToDbConnectionOptionsBuilder();
              var jsonSerializer = ctx.GetRequiredService<IJsonSerializer>();

              var ms = new MappingSchema();
              MappingSchema.Default.SetDefaultFromEnumType(typeof(Enum), typeof(string));
              var mb = ms.GetFluentMappingBuilder();
              mb.Entity<MonitorSubscription>()
                .HasSchemaName("public")
                .Property(_ => _.Id).IsPrimaryKey(0).IsIdentity().HasSkipOnInsert()
                .Property(_ => _.Slug)
                .Property(_ => _.DiscordWebhookUrl)
                .Property(_ => _.AllowedTargets)
                .HasConversionFunc(list => jsonSerializer.Serialize(list),
                  json => jsonSerializer.Deserialize<IList<string>>(json) ?? new List<string>())
                .HasDbType("jsonb");

              var options = connBuilder
                .UsePostgreSQL(cfg.GetConnectionString("PostgreSQL"))
                .UseMappingSchema(ms)
                .ConfigureTracesWithDefaults(ctx)
                .Build<MonitorSubscribersDbConnection>();

              using var conn = new MonitorSubscribersDbConnection(options);
              var sp = conn.DataProvider.GetSchemaProvider();
              var schema = sp.GetSchema(conn);
              var subscriptionsTableName = conn.Subscriptions.TableName;
              if (!schema.Tables.Exists(_ => _.TableName == subscriptionsTableName))
              {
                conn.CreateTable<MonitorSubscription>();
              }

              return options;
            });
        });
  }
}