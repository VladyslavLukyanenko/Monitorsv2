using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Configuration;
using ProjectMonitors.Monitor.Domain;

namespace ProjectMonitors.Monitor.Infra
{
  public class Linq2DbProductRepository : IProductRepository
  {
    private readonly LinqToDbConnectionOptions<ProductDbConnection> _options;

    public Linq2DbProductRepository(LinqToDbConnectionOptions<ProductDbConnection> options)
    {
      _options = options;
    }

    public async ValueTask<ProductRef?> GetRefAsync(string targetId, CancellationToken ct)
    {
      await using var conn = CreateConn();
      return await conn.Products.SingleOrDefaultAsync(_ => _.Target == targetId, ct);
    }

    private ProductDbConnection CreateConn() => new(_options);

    public async ValueTask CreateAsync(ProductRef productRef, CancellationToken ct)
    {
      await using var conn = CreateConn();
      await conn.InsertAsync(productRef, token: ct);
    }

    public async ValueTask<bool> AreStatusChangedAsync(string targetId, ProductStatus expectedStatus, CancellationToken ct)
    {
      await using var conn = CreateConn();
      return await conn.Products.AnyAsync(_ => _.Target == targetId && _.Status != expectedStatus, ct);
    }

    public async ValueTask ChangeStatusAsync(string targetId, ProductStatus status, CancellationToken ct)
    {
      await using var conn = CreateConn();
      await conn.Products.Where(_ => _.Target == targetId && _.Status != status)
        .Set(_ => _.Status, status)
        .UpdateAsync(ct);
    }
  }
}