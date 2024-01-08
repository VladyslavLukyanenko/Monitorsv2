using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectMonitors.SeedWork.Domain
{
  public interface IBinarySerializer
  {
    ValueTask SerializeAsync<T>(Stream output, T value, CancellationToken ct = default);
    ValueTask<T?> DeserializeAsync<T>(Stream input, CancellationToken ct = default);
    ValueTask<T?> DeserializeAsync<T>(ReadOnlyMemory<byte> input, CancellationToken ct = default);
  }
}