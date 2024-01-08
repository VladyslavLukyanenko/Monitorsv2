using Docker.DotNet;

namespace MonitorsPanel.Core.Manager.Services
{
  public interface IDockerClientsProvider
  {
    DockerClient GetLocalDockerClient();
    void AddClient(ServerInstance instance);
    void RemoveClient(ServerInstance instance);
    DockerClient GetClient(ServerInstance serverInstance);
    bool TryGetClient(ServerInstance serverInstance, out DockerClient client);
  }
}