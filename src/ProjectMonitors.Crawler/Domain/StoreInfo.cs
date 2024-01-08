namespace ProjectMonitors.Crawler.Domain
{
  public class StoreInfo
  {
    /// <summary>
    /// Identifier of the store. Usually just domain name.
    /// </summary>
    public string Id { get; init; } = null!;

    public int TotalElementsCount { get; init; }
    public int PagesCount { get; init; }
    public string Query { get; init; }
  }
}