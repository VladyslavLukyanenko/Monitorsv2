using System;
using System.Collections.Generic;
using Docker.DotNet;

namespace MonitorsPanel.Core.Manager.Services
{
  public class DockerClientsProvider : IDockerClientsProvider
  {
    private readonly IDictionary<string, DockerClient> _clientsCache = new Dictionary<string, DockerClient>();

    public DockerClient GetLocalDockerClient()
    {
      return new DockerClientConfiguration(new Uri("http://localhost:2375")).CreateClient();
    }

    public void AddClient(ServerInstance instance)
    {
      if (_clientsCache.ContainsKey(instance.Id))
      {
        throw new InvalidOperationException($"Client '{instance.Id}' already added.");
      }

      var client = new DockerClientConfiguration(instance.DockerRemoteApiUrl).CreateClient();
      _clientsCache[instance.Id] = client;
    }

    public void RemoveClient(ServerInstance instance)
    {
      
    }

    public DockerClient GetClient(ServerInstance serverInstance)
    {
      if (!TryGetClient(serverInstance, out var client))
      {
        throw new InvalidOperationException($"Client for '{serverInstance.DockerRemoteApiUrl}' not connected.");
      }

      return client;
    }

    public bool TryGetClient(ServerInstance serverInstance, out DockerClient client)
    {
      client = null;
      return _clientsCache.TryGetValue(serverInstance.Id, out client);
    }
  }
}