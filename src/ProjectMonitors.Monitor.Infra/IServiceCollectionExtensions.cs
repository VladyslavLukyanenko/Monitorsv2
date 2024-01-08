using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Mapping;
using Microsoft.Extensions.DependencyInjection;
using ProjectMonitors.Monitor.App;
using ProjectMonitors.Monitor.Domain;
using ProjectMonitors.Monitor.Infra.Config;
using ProjectMonitors.SeedWork;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Monitor.Infra
{
  // ReSharper disable once InconsistentNaming
  public static class IServiceCollectionExtensions
  {
    public static IServiceCollection AddMonitorsFrameworkWithDefaults(this IServiceCollection services,
      string monitorSlug)
    {
      return services
        .AddSingleton(_ => new MonitorInfo
        {
          Name = AppConstants.AppName,
          Slug = monitorSlug,
        })
        .AddConfiguredRabbitMq()
        .AddConfiguredCoreServices()
        .AddConfiguredTelemetry()
        .AddMonitorsFramework();
    }

    public static IServiceCollection AddMonitorsFramework(this IServiceCollection services)
    {
      services
        .AddSingleton<IWatchersManager, WatchersManager>()
        .AddSingleton<IProductRepository, Linq2DbProductRepository>()
        .AddSingleton<IMonitorSettingsRepository, Linq2DbMonitorSettingsRepository>()
        .AddSingleton<IWatcherFactory, GenericWatcherFactory>()
        .AddSingleton<INotificationPublisher, RabbitMqNotificationPublisher>()
        .AddSingleton<IMonitorHttpClientFactory, MonitorHttpClientFactory>()
        .AddSingleton<IAntibotProtectionSolverProvider, DiBasedAntibotProtectionSolverProvider>()
        .AddSingleton<IProductStatusFetcherFactoryProvider, DiBasedProductStatusFetcherFactoryProvider>()
        .AddSingleton(ctx =>
        {
          var connStrs = ctx.GetRequiredService<ConnectionStrings>();
          // var specFactory = ctx.GetRequiredService<IProductUrlSpecCodec>();
          var jsonSerializer = ctx.GetRequiredService<IJsonSerializer>();
          var activitySource = ctx.GetRequiredService<ActivitySource>();
          using var __ = activitySource.StartActivity("prepare_monitor_settings_db");
          var connBuilder = new LinqToDbConnectionOptionsBuilder();

          var ms = new MappingSchema();
          MappingSchema.Default.SetDefaultFromEnumType(typeof(Enum), typeof(string));
          var mb = ms.GetFluentMappingBuilder();
          mb.Entity<MonitorSettings>()
            .HasSchemaName("public")
            .Property(_ => _.MonitorSlug).IsPrimaryKey(0)
            .Property(_ => _.MonitorName).IsPrimaryKey(1)
            .Property(_ => _.AntibotConfig.ApiKey).IsColumn()
            .Property(_ => _.AntibotConfig.CookieLifetime).IsColumn()
            .Property(_ => _.AntibotConfig.ProtectProvider).IsColumn()
            .Property(_ => _.AntibotConfig.AdditionalConfig).IsColumn()
            .HasConversionFunc(e => jsonSerializer.Serialize(e),
              json => jsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>())
            .HasDbType("jsonb")
            .Property(_ => _.Proxies)
            .HasConversionFunc(e => jsonSerializer.Serialize(e),
              json => jsonSerializer.Deserialize<List<Uri>>(json) ?? new List<Uri>())
            .HasDbType("jsonb")
            .Property(_ => _.UserAgents)
            .HasConversionFunc(e => jsonSerializer.Serialize(e),
              json => jsonSerializer.Deserialize<IList<string>>(json) ?? new List<string>())
            .HasDbType("jsonb")
            .Property(_ => _.Targets)
            .HasConversionFunc(s => jsonSerializer.Serialize(s),
              json => jsonSerializer.Deserialize<IList<WatchTarget>>(json) ?? new List<WatchTarget>())
            .HasDbType("jsonb");

          var options = connBuilder
            .UsePostgreSQL(connStrs.PostgreSql)
            .UseMappingSchema(ms)
            .ConfigureTracesWithDefaults(ctx)
            .Build<MonitorSettingsDbConnection>();

          using var conn = new MonitorSettingsDbConnection(options);
          var sp = conn.DataProvider.GetSchemaProvider();
          var schema = sp.GetSchema(conn);
          var monitorSettingsTableName = conn.Settings.TableName;
          if (!schema.Tables.Exists(_ => _.TableName == monitorSettingsTableName))
          {
            conn.CreateTable<MonitorSettings>();
          }

          return options;
        })
        .AddSingleton(ctx =>
        {
          var connStrs = ctx.GetRequiredService<ConnectionStrings>();
          var connBuilder = new LinqToDbConnectionOptionsBuilder();
          var activitySource = ctx.GetRequiredService<ActivitySource>();
          using var __ = activitySource.StartActivity("prepare_local_db");

          var ms = new MappingSchema();
          MappingSchema.Default.SetDefaultFromEnumType(typeof(Enum), typeof(string));
          var mb = ms.GetFluentMappingBuilder();
          mb.Entity<ProductRef>()
            .Property(_ => _.Target).IsPrimaryKey(0)
            .Property(_ => _.Status).IsPrimaryKey(1);

          var options = connBuilder
            .UseSQLiteOfficial(connStrs.Sqlite)
            .UseMappingSchema(ms)
            .ConfigureTracesWithDefaults(ctx)
            .Build<ProductDbConnection>();

          using var conn = new ProductDbConnection(options);
          var sp = conn.DataProvider.GetSchemaProvider();
          var schema = sp.GetSchema(conn);
          if (!schema.Tables.Any())
          {
            conn.CreateTable<ProductRef>();
          }

          return options;
        });


      var fetcherFactoryTypes = typeof(MonitorAttribute).Assembly
        .DefinedTypes
        .Where(type => type.IsAssignableTo(typeof(IProductStatusFetcherFactory)))
        .Where(t => !t.IsAbstract)
        .ToArray();

      foreach (var fetcherFactoryType in fetcherFactoryTypes)
      {
        services.AddSingleton(typeof(IProductStatusFetcherFactory), fetcherFactoryType);
      }

      return services;
    }
  }
}