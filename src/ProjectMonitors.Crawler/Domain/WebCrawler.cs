using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ProjectMonitors.SeedWork;
using ProjectMonitors.SeedWork.Domain;

namespace ProjectMonitors.Crawler.Domain
{
  public class WebCrawler : ICrawler
  {
    private const int WorkersCount = 4;
    private static readonly TimeSpan ItemsCountCheckDelay = TimeSpan.FromSeconds(1);
    private static readonly SemaphoreSlim ModificationGates = new(1, 1);

    private readonly ILogger<WebCrawler> _logger;
    private readonly IProductPageRepository _productPageRepository;
    private readonly IScraper _scraper;

    private CancellationTokenSource _reindexCts = new();

    private CancellationTokenSource _quickAddDetectorCts = new();
    // private TaskCompletionSource _quickChangeDetectorSync = new();

    public WebCrawler(ILogger<WebCrawler> logger, IProductPageRepository productPageRepository,
      IScraper scraper)
    {
      _logger = logger;
      _productPageRepository = productPageRepository;
      _scraper = scraper;
    }

    public async ValueTask IndexAsync(CancellationToken ct = default)
    {
      var pageUrls = await _scraper.ExtractPageUrlsAsync(ct);
      await foreach (var page in ParseProductPagesAsync(pageUrls, _scraper.Config.ItemsPerPage, ct))
      {
        await _productPageRepository.ReplaceAsync(page, ct);
      }
    }

    private async IAsyncEnumerable<ProductPage> ParseProductPagesAsync(IEnumerable<Uri> pageUrls,
      int requestsItemsPerPage, [EnumeratorCancellation] CancellationToken ct)
    {
      var pageIdx = 0;
      foreach (var pageUrl in pageUrls)
      {
        yield return await FetchProductPageAsync(pageUrl, pageIdx, requestsItemsPerPage, ct);
        ++pageIdx;
      }
    }

    private async Task<ProductPage> FetchProductPageAsync(Uri pageUrl, int pageIdx, int requestsItemsPerPage,
      CancellationToken ct)
    {
      return new()
      {
        Products = await _scraper.ParseProductsAsync(pageUrl, pageIdx, ct).ToListAsync(ct),
        Url = pageUrl,
        PageIdx = pageIdx,
        ItemsPerPage = requestsItemsPerPage
      };
    }

    public void StartWatchers(CancellationToken ct = default)
    {
      StartQuickAddDetector(ct);
      StartBackgroundReindex(ct);
    }

    public async ValueTask ClearIndexAsync(CancellationToken ct = default)
    {
      await _productPageRepository.ClearAsync(ct);
    }

    private void StartBackgroundReindex(CancellationToken ct = default)
    {
      _ = Task.Factory.StartNew(async () =>
      {
        while (!ct.IsCancellationRequested)
        {
          _reindexCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
          var cts = _reindexCts;
          try
          {
            // find page with new/removed product
            var changesDescriptor = await ReindexAsync(cts.Token);
            if (changesDescriptor != IndexChangeDescriptor.NoChanges)
            {
              await ProductsUpdated.InvokeIfNotEmptyAsync(this,
                new ProductsUpdatedEventArgs(changesDescriptor.AddedProducts, changesDescriptor.RemovedProducts,
                  CancellationToken.None));
            }

            // await _quickChangeDetectorSync.Task;
          }
          catch (OperationCanceledException)
          {
            _logger.LogWarning("Cancelled due to changes detected");
          }
          catch (Exception exc)
          {
            _logger.LogError(exc, "An error occured on check products count");
          }

          await Task.Delay(ItemsCountCheckDelay, ct);
        }
      }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private async ValueTask<IndexChangeDescriptor> ReindexAsync(CancellationToken ct)
    {
      var pages = await _scraper.ExtractPageUrlsAsync(ct);
      // _logger.LogDebug("Start reindex iteration");
      // var timer = Stopwatch.StartNew();
      var gates = new SemaphoreSlim(WorkersCount, WorkersCount);
      var grabbedPages = new ProductPage[pages.Count];
      var tasks = pages.Select(async (productPageUrl, ix) =>
      {
        try
        {
          await gates.WaitAsync(CancellationToken.None).ConfigureAwait(false);
          if (ct.IsCancellationRequested)
          {
            return;
          }

          var page = await FetchProductPageAsync(productPageUrl, ix, _scraper.Config.ItemsPerPage, ct)
            .ConfigureAwait(false);
          grabbedPages[ix] = page;
        }
        finally
        {
          gates.Release();
        }
      });

      await Task.WhenAll(tasks).ConfigureAwait(false);
      // var elapsed = timer.Elapsed;
      // timer.Stop();
      // _logger.LogDebug("Reindex iteration finished in {Elapsed}", elapsed);

      if (ct.IsCancellationRequested)
      {
        return IndexChangeDescriptor.NoChanges;
      }

      try
      {
        await ModificationGates.WaitAsync(CancellationToken.None);


        var indexedPages = await _productPageRepository.GetListAsync(ct).ConfigureAwait(false);
        var initialProducts = indexedPages.SelectMany(_ => _.Products).ToArray();
        var detectedProducts = grabbedPages.SelectMany(_ => _.Products).ToArray();

        var newProducts = detectedProducts.Except(initialProducts).ToArray();
        var removedProducts = initialProducts.Except(detectedProducts).ToArray();

        if (!newProducts.Any() && !removedProducts.Any() && !initialProducts.SequenceEqual(detectedProducts))
        {
          _logger.LogDebug("Reorder detected");
        }

        await _productPageRepository.ReplaceRangeAsync(grabbedPages, ct).ConfigureAwait(false);

        if (!newProducts.Any() && !removedProducts.Any())
        {
          return IndexChangeDescriptor.NoChanges;
        }

        var oldQuickAddDetectorCts = _quickAddDetectorCts;
        _quickAddDetectorCts = new CancellationTokenSource();
        oldQuickAddDetectorCts.Cancel();

        return new IndexChangeDescriptor(newProducts, removedProducts);
      }
      finally
      {
        ModificationGates.Release();
      }
    }

    private void StartQuickAddDetector(CancellationToken ct)
    {
      _ = Task.Factory.StartNew(async () =>
      {
        while (!ct.IsCancellationRequested)
        {
          try
          {
            // find page with new/removed product
            var changesDescriptor = await QuickDetectAddsAsync(ct);
            if (changesDescriptor != IndexChangeDescriptor.NoChanges)
            {
              await ProductsUpdated.InvokeIfNotEmptyAsync(this,
                new ProductsUpdatedEventArgs(changesDescriptor.AddedProducts, changesDescriptor.RemovedProducts,
                  CancellationToken.None));
            }
          }
          catch (OperationCanceledException)
          {
            _logger.LogWarning("Quick change detection cancelled");
          }
          catch (Exception exc)
          {
            _logger.LogError(exc, "An error occured on check products count");
          }

          await Task.Delay(ItemsCountCheckDelay, ct);
        }
      }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private async ValueTask<IndexChangeDescriptor> QuickDetectAddsAsync(CancellationToken token = default)
    {
      var indexedCount = await _productPageRepository.ProductsCountAsync(token);
      var currentCount = await _scraper.GetProductsCountAsync(token);
      if (currentCount == indexedCount && indexedCount > 0)
      {
        return IndexChangeDescriptor.NoChanges;
      }

      try
      {
        await ModificationGates.WaitAsync(CancellationToken.None);
        _quickAddDetectorCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        var ct = _quickAddDetectorCts.Token;
        var oldReindexCts = _reindexCts;

        _logger.LogInformation("Changes detected. Synchronizing index...");

        IList<Product>? initialKnownProducts = null;
        var addedProducts = new List<Product>();
        var removedProducts = new List<Product>();
        var searchContext = new SearchForChangesContext(indexedCount, currentCount);

        await foreach (var cursor in SearchForChangesAsync(searchContext, ct))
        {
          if (cursor.ChangesCount <= 0)
          {
            break;
          }

          if (oldReindexCts != null)
          {
            _reindexCts = new CancellationTokenSource();
            oldReindexCts.Cancel();
            oldReindexCts = null;
          }

          initialKnownProducts ??= cursor.AllIndexedProducts;
          var newProducts = cursor.FreshPage.Products.Except(cursor.AllIndexedProducts).ToArray();
          if (!newProducts.Any())
          {
            cursor.Backward();
            continue;
          }

          if (newProducts.Any())
          {
            cursor.IndexedPages = ProductPage.InsertArranged(cursor.AllIndexedProducts, newProducts,
              _scraper.Config.ItemsPerPage, _scraper.CreatePageUrl);
            cursor.UpdateChangesCount(newProducts.Length * -1);
            addedProducts.AddRange(newProducts);
          }

          // no new products on this page. it means new products added on some prev page
          if (newProducts.Length == 0)
          {
            cursor.Backward();
          }
        }

        initialKnownProducts ??= Array.Empty<Product>();
        removedProducts = removedProducts.Except(addedProducts).ToList();
        addedProducts = addedProducts.Except(initialKnownProducts).ToList();

        _logger.LogDebug("Changes processed. New - {NewProductsCount}, Removed - {RemovedProductsCount}",
          addedProducts.Count, removedProducts.Count);
        return new IndexChangeDescriptor(addedProducts.Except(initialKnownProducts), removedProducts);
      }
      finally
      {
        ModificationGates.Release();
      }
    }

    private async IAsyncEnumerable<ChangeDetectorCursor> SearchForChangesAsync(
      SearchForChangesContext ctx, [EnumeratorCancellation] CancellationToken ct)
    {
      // cases
      // product added
      // product removed
      // one product removed and two products added
      // single product replaced by other one
      // removed products will be detected during full reindex
      // because product can be reordered and we can send miss-notification
      var ip = await _productPageRepository.GetListAsync(ct);

      var prevIndex = default(BinarySearchIndex);
      var initialIndex = new BinarySearchIndex(0,
        Math.Max(CalculatePagesCount(ctx.IndexedProductsCount), CalculatePagesCount(ctx.DetectedProductsCount)) - 1);

      var changesCount = ctx.DetectedProductsCount - ctx.IndexedProductsCount;
      var cursor = new ChangeDetectorCursor(initialIndex, ip, changesCount);

      while (cursor.Index != prevIndex)
      {
        prevIndex = cursor.Index;
        var indexedPage = cursor.IndexedPages.Count <= cursor.Index.Middle
          ? null
          : cursor.IndexedPages[cursor.Index.Middle];

        var freshPage = await ctx.Cache.GetOrAdd(cursor.Index.Middle, () =>
        {
          if (indexedPage == null)
          {
            var url = _scraper.CreatePageUrl(cursor.Index.Middle, _scraper.Config.ItemsPerPage);
            return FetchProductPageAsync(url, cursor.Index.Middle, _scraper.Config.ItemsPerPage, ct);
          }

          return FetchProductPageAsync(indexedPage.Url, indexedPage.PageIdx, indexedPage.ItemsPerPage, ct);
        });
        if (indexedPage == freshPage)
        {
          cursor.Forward();
          continue;
        }

        cursor.FreshPage = freshPage;
        cursor.IndexedPage = indexedPage;
        yield return cursor;

        await _productPageRepository.ReplaceRangeAsync(cursor.IndexedPages, ct);
        if (cursor.ChangesCount == 0)
        {
          yield break;
        }

        cursor.UpdateIndexedProducts();
        // on next iteration let's check if there everything in sync now
      }

      int CalculatePagesCount(int productsCount) =>
        (int) Math.Ceiling(productsCount / (double) _scraper.Config.ItemsPerPage);
    }

    public event AsyncEventHandler<ProductsUpdatedEventArgs>? ProductsUpdated;
  }
}