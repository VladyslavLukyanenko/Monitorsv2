using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ProjectMonitors.SeedWork.Domain
{
  public class ProductSummary
  {
    private readonly List<ProductAttribute> _attributes = new();

    [JsonPropertyName("sku")] public string Sku { get; init; } = null!;
    [JsonPropertyName("title")] public string Title { get; init; } = null!;
    [JsonPropertyName("picture")] public string? Picture { get; init; }
    [JsonPropertyName("price")] public string? Price { get; set; }
    [JsonPropertyName("pageUri")] public Uri? PageUrl { get; init; }

    [JsonPropertyName("attributes")]
    public IReadOnlyCollection<ProductAttribute> Attributes
    {
      get => _attributes;
      init => _attributes = value.ToList();
    }

    [JsonPropertyName("links")] public IReadOnlyCollection<ProductLink> Links { get; init; } = new List<ProductLink>();

    public void UpdateAttr(ProductAttribute attr)
    {
      var ix = _attributes.FindLastIndex(_ => _.Name == attr.Name);
      if (ix == -1)
      {
        return;
      }

      _attributes[ix] = attr;
    }
  }
}