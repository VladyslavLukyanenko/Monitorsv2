using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MonitorsPanel.Core.Manager
{
  public interface IImagesRuntimeInfoService
  {
    Task RefreshStateAsync(IEnumerable<ServerInstance> nodes, CancellationToken ct = default);
  }
}