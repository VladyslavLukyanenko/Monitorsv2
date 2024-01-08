using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.SeedWork.Infra
{
  public class SystemTextJsonSerializer : IJsonSerializer
  {
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public SystemTextJsonSerializer(JsonSerializerOptions jsonSerializerOptions)
    {
      _jsonSerializerOptions = jsonSerializerOptions;
    }

    public ValueTask<T?> DeserializeAsync<T>(string json, CancellationToken ct = default)
    {
      var r = JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
      return ValueTask.FromResult(r);
    }

    public async ValueTask<T?> DeserializeAsync<T>(Stream jsonStream, CancellationToken ct = default)
    {
      return await JsonSerializer.DeserializeAsync<T>(jsonStream, _jsonSerializerOptions, ct);
    }

    public ValueTask<T?> DeserializeAsync<T>(ReadOnlyMemory<byte> inMemoryJson, CancellationToken ct = default)
    {
      return DeserializeAsync<T>(new MemoryStream(inMemoryJson.ToArray()), ct);
    }

    public T? Deserialize<T>(string json)
    {
      return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
    }

    public async ValueTask<string> SerializeAsync<T>(T obj, CancellationToken ct = default)
    {
      var mem = new MemoryStream();
      await SerializeAsync(mem, obj, ct);
      return Encoding.UTF8.GetString(mem.ToArray());
    }

    public string Serialize<T>(T obj)
    {
      return JsonSerializer.Serialize(obj, _jsonSerializerOptions);
    }

    public async ValueTask SerializeAsync<T>(Stream utf8Stream, T obj, CancellationToken ct = default)
    {
      await JsonSerializer.SerializeAsync(utf8Stream, obj, _jsonSerializerOptions, ct);
    }
  }
}