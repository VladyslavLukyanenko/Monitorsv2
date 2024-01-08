namespace MonitorsPanel.Core.Manager.Primitives
{
  public interface IEntity<out TKey>
  {
    TKey Id { get; }
  }
}