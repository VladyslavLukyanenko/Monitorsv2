using System;
using System.Collections.Generic;
using System.Diagnostics;
using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Mapping;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ProjectMonitors.SeedWork;
using ProjectMonitors.SeedWork.Domain;
using ProjectMonitors.Crawler.Config;
using ProjectMonitors.Crawler.Domain;
using ProjectMonitors.Crawler.Infra;
using ProjectMonitors.Crawler.Sites.FakeShop;

namespace ProjectMonitors.Crawler
{
  public class Startup
  {
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
      services
        .Configure<ConnectionStrings>(_configuration.GetSection("ConnectionStrings"))
        .AddSingleton(ctx => ctx.GetRequiredService<IOptions<ConnectionStrings>>().Value)
        .AddConfiguredRabbitMq()
        .AddConfiguredCoreServices()
        .AddConfiguredTelemetry()
        .AddSingleton<IProductPageRepository, Linq2DbProductPageRepository>()
        .AddSingleton<IScraperHttpClientFactory, ScraperHttpClientFactory>()
        .AddSingleton<ICrawler, WebCrawler>()
        .AddSingleton<IScraper, FakeShopScraper>()
        // .AddSingleton<IScraper, FanaticsScraper>()
        .AddSingleton(_ => new ScraperConfig
        {
          Query = "Nike"
        })
        .AddSingleton(ctx =>
        {
          var connStrs = ctx.GetRequiredService<ConnectionStrings>();
          var jsonSerializer = ctx.GetRequiredService<IJsonSerializer>();
          var activitySource = ctx.GetRequiredService<ActivitySource>();
          using var __ = activitySource.StartActivity("prepare_monitor_settings_db");
          var connBuilder = new LinqToDbConnectionOptionsBuilder();

          var ms = new MappingSchema();
          MappingSchema.Default.SetDefaultFromEnumType(typeof(Enum), typeof(string));
          var mb = ms.GetFluentMappingBuilder();
          mb.Entity<ProductPage>()
            .HasSchemaName("public")
            .Property(_ => _.Url)
            .HasConversionFunc(url => url.ToString(), raw => new Uri(raw, UriKind.RelativeOrAbsolute))
            .IsPrimaryKey()
            .HasDbType("varchar(2048)")
            .Property(_ => _.Products)
            .HasConversionFunc(p => jsonSerializer.Serialize(p),
              json => jsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>())
            .HasDbType("jsonb");

          var options = connBuilder
            .UsePostgreSQL(connStrs.PostgreSql)
            .UseMappingSchema(ms)
            .ConfigureTracesWithDefaults(ctx)
            .Build<CrawlerDbConnection>();

          using var conn = new CrawlerDbConnection(options);
          var sp = conn.DataProvider.GetSchemaProvider();
          var schema = sp.GetSchema(conn);
          var tableName = conn.ProductPages.TableName;
          if (!schema.Tables.Exists(_ => _.TableName == tableName))
          {
            conn.CreateTable<ProductPage>();
          }

          return options;
        });
    }
  }
}