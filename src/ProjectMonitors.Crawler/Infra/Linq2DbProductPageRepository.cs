using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Data;
using ProjectMonitors.Crawler.Domain;

namespace ProjectMonitors.Crawler.Infra
{
  public class Linq2DbProductPageRepository : IProductPageRepository
  {
    private readonly LinqToDbConnectionOptions<CrawlerDbConnection> _options;

    public Linq2DbProductPageRepository(LinqToDbConnectionOptions<CrawlerDbConnection> options)
    {
      _options = options;
    }

    public async ValueTask ReplaceAsync(ProductPage page, CancellationToken ct = default)
    {
      await using var conn = new CrawlerDbConnection(_options);
      await conn.ProductPages.Where(_ => _.Url == page.Url).DeleteAsync(ct);
      await conn.InsertAsync(page, token: ct);
    }

    public async ValueTask<int> ProductsCountAsync(CancellationToken ct = default)
    {
      var pages = await GetListAsync(ct);
      return pages.Select(_ => _.Products.Count).Sum();
    }

    public async ValueTask<IList<ProductPage>> GetListAsync(CancellationToken ct = default)
    {
      await using var conn = new CrawlerDbConnection(_options);
      return await conn.ProductPages.OrderBy(_ => _.PageIdx).ToListAsync(ct);
    }

    public async ValueTask ClearAsync(CancellationToken ct = default)
    {
      await using var conn = new CrawlerDbConnection(_options);
      await conn.ProductPages.DeleteAsync(ct);
    }

    public async ValueTask ReplaceRangeAsync(IEnumerable<ProductPage> pages, CancellationToken ct = default)
    {
      await using var conn = new CrawlerDbConnection(_options);
      var tx = await conn.BeginTransactionAsync(ct);
      try
      {
        await conn.ProductPages.DeleteAsync(ct);
        await conn.BulkCopyAsync(pages, ct);
        
        await tx.CommitAsync(ct);
      }
      catch (Exception)
      {
        await tx.RollbackAsync(ct);
      }
      // await using var tmp =
      //   await conn.CreateTempTableAsync<ProductPage>("temp_table_" + Guid.NewGuid().ToString("N"),
      //     cancellationToken: ct);
      // var records = from target in conn.GetTable<ProductPage>()
      //   from source in tmp.InnerJoin(t => t.Url == target.Url)
      //   select new {target, changed = source};
      //
      // await records.Set(_ => _.target.Products, _ => _.changed.Products)
      //   .Set(_ => _.target.PageIdx, _ => _.changed.PageIdx)
      //   .Set(_ => _.target.ItemsPerPage, _ => _.changed.ItemsPerPage)
      //   .Set(_ => _.target.UpdatedAt, _ => Sql.CurrentTimestamp)
      //   .UpdateAsync(ct);



    }
  }
}