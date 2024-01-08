using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using OpenTelemetry.Trace;
using ProjectMonitors.SeedWork;

namespace ProjectMonitors.Monitor.Domain
{
  public class StatusFetchResult
  {
    private readonly List<IWatchStatus> _statuses = new();
    private MemoryStream _responseStream = null!;
    private string _responseStr = null!;

    private StatusFetchResult()
    {
    }

    public static StatusFetchResult NewEmpty() => new();

    public static async ValueTask<Result<StatusFetchResult>> ProcessResultAsync(HttpRequestMessage request,
      HttpClient client, CancellationToken ct,
      Func<StatusFetchResult, ValueTask<Result<StatusFetchResult>>> fulfilExecutor,
      Func<HttpResponseMessage, bool>? isSuccessPredicate = null)
    {
      var fetcherActivity = Activity.Current;
      fetcherActivity?.SetHttpRequestMessage(request);
      try
      {
        var response = await client.SendAsync(request, ct);
        var r = new StatusFetchResult
        {
          Response = response
        };

        try
        {
          var stream = await response.Content.ReadAsStreamAsync(ct);
          r._responseStream = new MemoryStream();
          await stream.CopyToAsync(r._responseStream, ct);

          r._responseStr = Encoding.UTF8.GetString(r._responseStream.ToArray());
          r._responseStream.Seek(0, SeekOrigin.Begin);
          fetcherActivity?.SetTag("raw_response", r._responseStr);
        }
        catch (Exception exc)
        {
          fetcherActivity.RecordException(exc);
          throw;
        }

        fetcherActivity?.SetHttpResponseMessage(response);
        if (isSuccessPredicate != null && !isSuccessPredicate(response)
            || isSuccessPredicate == null && !response.IsSuccessStatusCode)
        {
          string errorDesc = response.StatusCode == HttpStatusCode.TooManyRequests
            ? "rate limited"
            : "non success status code";

          return Result.Failure<StatusFetchResult>(errorDesc);
        }

        return await fulfilExecutor(r);
      }
      catch (Exception exc)
      {
        fetcherActivity.RecordException(exc);
        throw;
      }
    }

    public static async ValueTask<Result<StatusFetchResult>> ProcessResult(HttpRequestMessage request,
      HttpClient client, CancellationToken ct,
      Func<StatusFetchResult, Result<StatusFetchResult>> fulfilExecutor,
      Func<HttpResponseMessage, bool>? isSuccessPredicate = null) =>
      await ProcessResultAsync(request, client, ct, r => ValueTask.FromResult(fulfilExecutor(r)), isSuccessPredicate);

    public void AddStatus(string id, bool available, TimeSpan? delayRequest = null)
    {
      _statuses.Add(new WatchStatus(id, available, delayRequest));
    }

    public IReadOnlyList<IWatchStatus> Targets => _statuses.AsReadOnly();
    public Stream RawResponse => _responseStream;

    public HttpResponseMessage Response { get; private set; } = null!;

    public string GetResponseAsString() => _responseStr;
  }
}