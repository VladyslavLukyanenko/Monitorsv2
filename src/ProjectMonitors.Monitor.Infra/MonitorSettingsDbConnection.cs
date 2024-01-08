using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Data;
using ProjectMonitors.Monitor.Domain;
using ProjectMonitors.SeedWork;

namespace ProjectMonitors.Monitor.Infra
{
  public class MonitorSettingsDbConnection : DataConnection
  {
    public MonitorSettingsDbConnection(LinqToDbConnectionOptions<MonitorSettingsDbConnection> options)
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

    public ITable<MonitorSettings> Settings => GetTable<MonitorSettings>();
  }
}