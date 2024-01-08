using System;
using System.Collections.Generic;
using System.Linq;
using ProjectMonitors.Monitor.Domain;

namespace ProjectMonitors.Monitor.App
{
  public class DiBasedAntibotProtectionSolverProvider : IAntibotProtectionSolverProvider
  {
    private readonly IEnumerable<IAntibotProtectionSolver> _solvers;

    public DiBasedAntibotProtectionSolverProvider(IEnumerable<IAntibotProtectionSolver> solvers)
    {
      _solvers = solvers;
    }

    public IAntibotProtectionSolver? GetSolver(string provider)
    {
      return _solvers.FirstOrDefault(_ =>
        string.Equals(_.ProviderName, provider, StringComparison.InvariantCultureIgnoreCase));
    }
  }
}