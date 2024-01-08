using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Data;
using ProjectMonitors.SeedWork;
using ProjectMonitors.Crawler.Domain;

namespace ProjectMonitors.Crawler.Infra
{
  public class CrawlerDbConnection : DataConnection
  {
    public CrawlerDbConnection(LinqToDbConnectionOptions<CrawlerDbConnection> options)
      : base(options)
    {
      MappingSchema.EntityDescriptorCreatedCallback = (_, descriptor) =>
      {
        if (string.IsNullOrEmpty(descriptor.SchemaName))
        {
          return;
        }

        descriptor.TableName = descriptor.TableName.ToSnakeCase();
        foreach (var column in descriptor.Columns)
        {
          column.ColumnName = column.ColumnName.ToSnakeCase();
        }
      };
    }

    public ITable<ProductPage> ProductPages => GetTable<ProductPage>();
  }
}