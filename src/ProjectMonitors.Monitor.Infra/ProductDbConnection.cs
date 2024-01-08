using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Data;
using ProjectMonitors.SeedWork;
using ProjectMonitors.Monitor.Domain;

namespace ProjectMonitors.Monitor.Infra
{
  public class ProductDbConnection : DataConnection
  {
    public ProductDbConnection(LinqToDbConnectionOptions<ProductDbConnection> options)
      : base(options)
    {
      MappingSchema.EntityDescriptorCreatedCallback = (_, descriptor) =>
      {
        descriptor.TableName = descriptor.TableName.ToSnakeCase();
        foreach (var column in descriptor.Columns)
        {
          column.ColumnName = column.ColumnName.ToSnakeCase();
        }
      };
    }

    public ITable<ProductRef> Products => GetTable<ProductRef>();
  }
}