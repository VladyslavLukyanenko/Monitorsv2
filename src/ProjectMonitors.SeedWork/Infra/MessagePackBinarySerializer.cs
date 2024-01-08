using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.SeedWork.Infra
{
  public class MessagePackBinarySerializer : IBinarySerializer
  {
    private readonly MessagePackSerializerOptions _options;

    public MessagePackBinarySerializer(MessagePackSerializerOptions options)
    {
      _options = options;
    }

    public async ValueTask SerializeAsync<T>(Stream output, T value, CancellationToken ct = default)
    {
      await MessagePackSerializer.SerializeAsync(output, value, _options, ct);
    }

    public async ValueTask<T?> DeserializeAsync<T>(Stream input, CancellationToken ct = default)
    {
      return await MessagePackSerializer.DeserializeAsync<T?>(input, _options, ct);
    }

    public async ValueTask<T?> DeserializeAsync<T>(ReadOnlyMemory<byte> input, CancellationToken ct = default)
    {
      return await DeserializeAsync<T>(new MemoryStream(input.ToArray()), ct);
    }
  }
}