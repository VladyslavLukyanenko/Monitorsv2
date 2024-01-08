using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using CSharpFunctionalExtensions;
using ProjectMonitors.Monitor.Domain;

//
// namespace ProjectMonitors.Monitor.App.Sites.Amazon
// {
//   public class AmazonFetcher : IProductStatusFetcher
//   {
//     public const string OfferIdAttrName = "Offer Listing Id";
//     private readonly HttpClient _httpClient;
//     private readonly WatchTarget _target;
//     private readonly Dictionary<string, string?> _cartSubmitData;
//
//     public AmazonFetcher(HttpClient httpClient, WatchTarget target)
//     {
//       httpClient.DefaultRequestHeaders.Add("accept",
//         "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
//       _httpClient = httpClient;
//       _target = target;
//
//       var offerAttr = target.Products[target.Input].Attributes.FirstOrDefault(_ => _.Name == OfferIdAttrName);
//       if (offerAttr == null)
//       {
//         throw new ArgumentException($"Product attribute with name '{OfferIdAttrName}' is required");
//       }
//
//       _cartSubmitData = new Dictionary<string, string?>
//       {
//         {"CSRF", ""},
//         {"offerListingID", offerAttr.Value},
//         {"session-id", ""},
//         {"ASIN", target.Input},
//         {"isMerchantExclusive", "0"},
//         {"merchantID", "ATVPDKIKX0DER"},
//         {"isAddon", "0"},
//         {"nodeID", ""},
//         {"sellingCustomerID", ""},
//         {"qid", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture)},
//         {"sr", "8-1"},
//         {"storeID", ""},
//         {"tagActionCode", ""},
//         {"viewID", "glance"},
//         {"rebateId", ""},
//         {"ctaDeviceType", "desktop"},
//         {"ctaPageType", "detail"},
//         {"usePrimeHandler", "0"},
//         {"rsid", ""},
//         {"sourceCustomerOrgListID", ""},
//         {"sourceCustomerOrgListItemID", ""},
//         {"wlPopCommand", ""},
//         {"quantity", "1"},
//         {"submit.buy-now", "Submit"},
//         {"gr-dropdown-name", ""},
//         {"account-linking", "yes"},
//         {"verificationSessionID", ""},
//         {"itemCount", "7"},
//         {"custom-name.1.1", "isDevicePreRegistered"},
//         {"custom-value.1.1", "true"},
//       };
//     }
//
//     public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
//     {
//       var requestUri = "https://smile.amazon.com/gp/product/handle-buy-box/ref=dp_start-bbf_1_glance";
//       var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
//       {
//         
//         Content = new FormUrlEncodedContent(_cartSubmitData!)
//       };
//
//       RandomHeaders(request.Headers);
//
//       return await StatusFetchResult.ProcessResult(request, _httpClient, ct, result =>
//       {
//         var available = result.Response.Headers.Location?.ToString()
//           .Contains("/signin/", StringComparison.OrdinalIgnoreCase) == true;
//         result.AddStatus(_target.Input, available);
//
//         return result;
//       }, r => r.StatusCode == HttpStatusCode.Found || r.IsSuccessStatusCode);
//     }
//
//
//     void RandomHeaders(HttpHeaders dstHeader)
//     {
//       var rnd = new Random((int) DateTime.Now.Ticks);
//       var headers = new Dictionary<string, string>();
//       headers.Add("accept", "*/*");
//       headers.Add("accept-encoding", "gzip, deflate, br");
//       headers.Add("accept-language", "en-US,en;q=0.9");
//       headers.Add("cache-control", $"max-age = {Math.Floor(rnd.NextDouble() * 101)}0000000");
//       headers.Add("origin", "https://www.amazon.com");
//       headers.Add("referer",
//         $"https://www.amazon.com/dp/${_target.Input}/ref=sr_1_1?dchild=1&keywords=${_target.Input}&qid=${DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}&sr=8-1");
//       headers.Add("sec-ch-ua", "\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"90\", \"Google Chrome\";v=\"90\"");
//       headers.Add("sec-fetch-dest", "empty");
//       headers.Add("sec-fetch-mode", "cors");
//       headers.Add("sec-fetch-site", "same-origin");
//       headers.Add("sec-fetch-user", "?1");
//       headers.Add("user-agent",
//         $"Android {Math.Floor(rnd.NextDouble() * 200)};Mobile;rv:{Math.Floor(rnd.NextDouble() * 200)}.0");
//       headers.Add("x-amz-checkout-entry-referer-url",
//         $"https://www.amazon.com/dp/${_target.Input}/ref=sr_1_1?dchild=1&keywords=${_target.Input}&qid=${DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}&sr=8-1");
//       headers.Add("x-amz-support-custom-signin", "1");
//       headers.Add("x-amz-turbo-checkout-dp-url",
//         $"https://www.amazon.com/dp/${_target.Input}/ref=sr_1_1?dchild=1&keywords=${_target.Input}&qid=${DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}&sr=8-1");
//
//
//       var headers2 = new Dictionary<string, string>
//       {
//         // "content-type": 'application/x-www-form-urlencoded',
//         {"downlink", "10"},
//         {"ect", "4g"},
//         {"rtt", "50"},
//         {"sec-ch-ua-mobile", "?0"},
//         {"upgrade-insecure-requests", "1"},
//         {"x-requested-with", "XMLHttpRequest"},
//         {"cookie", (IntToStringFast(rnd.Next(10_000_000, 99_999_999), 36) + IntToStringFast(rnd.Next(10_000_000, 99_999_999), 36)).Substring(7)}
//       };
//
//       var randomNum1 = rnd.Next(0, headers2.Count);
//       for (var num = 0; num < randomNum1; num++)
//       {
//         var randomNum2 = rnd.Next(0, headers2.Count);
//         var p = headers2.ElementAt(randomNum2);
//         headers[p.Key] = p.Value;
//       }
//
//       foreach (var header in headers)
//       {
//         dstHeader.TryAddWithoutValidation(header.Key, header.Value);
//       }
//     }
//
//
//     private static readonly char[] FallbackBaseChars =
//     {
//       '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
//       'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w',
//       'x', 'y', 'z',
//       'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W',
//       'X', 'Y', 'Z',
//     };
//
//     /// <summary>
//     /// An optimized method using an array as buffer instead of 
//     /// string concatenation. This is faster for return values having 
//     /// a length > 1.
//     /// </summary>
//     public static string IntToStringFast(int value, int targetBase, char[]? baseChars = null)
//     {
//       baseChars ??= FallbackBaseChars;
//       // 32 is the worst cast buffer size for base 2 and int.MaxValue
//       int i = 32;
//       char[] buffer = new char[i];
//
//       do
//       {
//         buffer[--i] = baseChars[value % targetBase];
//         value = value / targetBase;
//       } while (value > 0);
//
//       char[] result = new char[32 - i];
//       Array.Copy(buffer, i, result, 0, 32 - i);
//
//       return new string(result);
//     }
//   }
// }

namespace ProjectMonitors.Monitor.App.Sites.Amazon
{
  public class AmazonFetcher : IProductStatusFetcher
  {
    private readonly HttpClient _httpClient;
    private readonly string _asin;

    public AmazonFetcher(HttpClient httpClient, string asin)
    {
      httpClient.DefaultRequestHeaders.Add("accept",
        "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
      _httpClient = httpClient;
      _asin = asin;
    }

    public async ValueTask<Result<StatusFetchResult>> FetchAsync(CancellationToken ct)
    {
      var requestUri = $"https://www.amazon.com/portal-migration/aod?asin={_asin}";
      var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
      RandomHeaders(request.Headers);
      return await StatusFetchResult.ProcessResultAsync(request, _httpClient, ct, async result =>
      {
        var ctx = BrowsingContext.New(Configuration.Default);
        var doc = await ctx.OpenAsync(_ => _.Content(result.RawResponse), ct);
        var captcha = doc.QuerySelector("#captchacharacters");
        if (captcha != null)
        {
          return Result.Failure<StatusFetchResult>("protected by captcha");
        }

        var available = doc.QuerySelector(".a-offscreen") == null;
        result.AddStatus(_asin, available);

        return result;
      });
    }


    void RandomHeaders(HttpHeaders dstHeader)
    {
      var rnd = new Random((int) DateTime.Now.Ticks);
      var headers = new Dictionary<string, string>();
      headers.Add("accept", "*/*");
      headers.Add("accept-encoding", "gzip, deflate, br");
      headers.Add("accept-language", "en-US,en;q=0.9");
      headers.Add("cache-control", $"max-age = {Math.Floor(rnd.NextDouble() * 101)}0000000");
      headers.Add("origin", "https://www.amazon.com");
      headers.Add("referer",
        $"https://www.amazon.com/dp/${_asin}/ref=sr_1_1?dchild=1&keywords=${_asin}&qid=${DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}&sr=8-1");
      headers.Add("sec-ch-ua", "\" Not A;Brand\";v=\"99\", \"Chromium\";v=\"90\", \"Google Chrome\";v=\"90\"");
      headers.Add("sec-fetch-dest", "empty");
      headers.Add("sec-fetch-mode", "cors");
      headers.Add("sec-fetch-site", "same-origin");
      headers.Add("sec-fetch-user", "?1");
      headers.Add("user-agent",
        $"Android {Math.Floor(rnd.NextDouble() * 200)};Mobile;rv:{Math.Floor(rnd.NextDouble() * 200)}.0");
      headers.Add("x-amz-checkout-entry-referer-url",
        $"https://www.amazon.com/dp/${_asin}/ref=sr_1_1?dchild=1&keywords=${_asin}&qid=${DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}&sr=8-1");
      headers.Add("x-amz-support-custom-signin", "1");
      headers.Add("x-amz-turbo-checkout-dp-url",
        $"https://www.amazon.com/dp/${_asin}/ref=sr_1_1?dchild=1&keywords=${_asin}&qid=${DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}&sr=8-1");


      var headers2 = new Dictionary<string, string>
      {
        // "content-type": 'application/x-www-form-urlencoded',
        {"downlink", "10"},
        {"ect", "4g"},
        {"rtt", "50"},
        {"sec-ch-ua-mobile", "?0"},
        {"upgrade-insecure-requests", "1"},
        {"x-requested-with", "XMLHttpRequest"},
        {
          "cookie",
          (IntToStringFast(rnd.Next(10_000_000, 99_999_999), 36)
           + IntToStringFast(rnd.Next(10_000_000, 99_999_999), 36)).Substring(7)
        }
      };

      var randomNum1 = rnd.Next(0, headers2.Count);
      for (var num = 0; num < randomNum1; num++)
      {
        var randomNum2 = rnd.Next(0, headers2.Count);
        var p = headers2.ElementAt(randomNum2);
        headers[p.Key] = p.Value;
      }

      foreach (var header in headers)
      {
        dstHeader.TryAddWithoutValidation(header.Key, header.Value);
      }
    }


    private static readonly char[] FallbackBaseChars =
    {
      '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
      'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w',
      'x', 'y', 'z',
      'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W',
      'X', 'Y', 'Z',
    };

    /// <summary>
    /// An optimized method using an array as buffer instead of 
    /// string concatenation. This is faster for return values having 
    /// a length > 1.
    /// </summary>
    public static string IntToStringFast(int value, int targetBase, char[]? baseChars = null)
    {
      baseChars ??= FallbackBaseChars;
      // 32 is the worst cast buffer size for base 2 and int.MaxValue
      int i = 32;
      char[] buffer = new char[i];

      do
      {
        buffer[--i] = baseChars[value % targetBase];
        value = value / targetBase;
      } while (value > 0);

      char[] result = new char[32 - i];
      Array.Copy(buffer, i, result, 0, 32 - i);

      return new string(result);
    }
  }
}