namespace ProjectMonitors.Monitor.Domain
{
  public class ProductRef
  {
    public ProductRef()
    {
    }

    public ProductRef(string target, ProductStatus status)
    {
      Target = target;
      Status = status;
    }

    public string Target { get; set; } = null!;
    public ProductStatus Status { get; set; }
  }
}