using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MonitorsPanel.Core.Manager.Primitives;

namespace MonitorsPanel.Web.ManagerApi.Infra
{
  public class DbContextUnitOfWork : IUnitOfWork
  {
    private readonly DbContext _context;

    public DbContextUnitOfWork(DbContext context)
    {
      _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken token = default)
    {
      return _context.SaveChangesAsync(token);
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken token = default)
    {
      await SaveChangesAsync(token);
      return true;
    }

    public async Task<ITransactionScope> BeginTransactionAsync(bool autoCommit = true,
      IsolationLevel isolationLevel = IsolationLevel.Unspecified, CancellationToken ct = default)
    {
      var tx = await _context.Database.BeginTransactionAsync(isolationLevel, ct);
      return new EfCoreTransactionScope(tx, autoCommit);
    }

    public void Dispose()
    {
      _context.Dispose();
    }
  }
}