using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Data;
using ProjectMonitors.Balancer.Domain;
using ProjectMonitors.SeedWork;

namespace ProjectMonitors.Balancer.Infra
{
  public class MonitorSubscribersDbConnection : DataConnection
  {
    public MonitorSubscribersDbConnection(LinqToDbConnectionOptions<MonitorSubscribersDbConnection> options)
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

    public ITable<MonitorSubscription> Subscriptions => GetTable<MonitorSubscription>();
  }
}