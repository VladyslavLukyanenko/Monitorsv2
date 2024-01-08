using System.Diagnostics.CodeAnalysis;

namespace MonitorsPanel.Web.ManagerApi.Foundation.Model
{
  public static class ApiContractExtensions
  {
    public static ApiContract<T> ToApiContract<T>([MaybeNull] this T self)
    {
      return new ApiContract<T>(self);
    }
  }
}