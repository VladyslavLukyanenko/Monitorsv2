namespace MonitorsPanel.Web.ManagerApi.Foundation.Config
{
  public class DataSourceConfig
  {
    public string PostgresConnectionString { get; set; }
    public RabbitMqConfig RabbitMq { get; set; }

    public int MaxRetryCount { get; set; }

    public class RabbitMqConfig
    {
      public string Host { get; set; }
      public ushort Port { get; set; }
      public string Username { get; set; }
      public string Password { get; set; }
    }
  }
}