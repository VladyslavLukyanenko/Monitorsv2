namespace ProjectMonitors.Monitor.Domain
{
  public interface IAntibotProtectionSolverProvider
  {
    IAntibotProtectionSolver? GetSolver(string provider);
  }
}