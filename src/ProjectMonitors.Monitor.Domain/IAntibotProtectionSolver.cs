using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectMonitors.Monitor.Domain
{
  public interface IAntibotProtectionSolver
  {
    string ProviderName { get; }

    ValueTask<Cookie?> SolveCookieAsync(Uri requestUri, AntibotProtectionConfig config, CancellationToken ct = default);

    ValueTask<Cookie?> SolveCaptchaAsync(Uri requestUri, AntibotProtectionConfig config,
      CancellationToken ct = default);
  }
}