﻿using System;
using System.Text.Json.Serialization;

#nullable disable
namespace ProjectMonitors.Monitor.App.Sites.Shopify
{
  public partial class ShopifyData
  {
    [JsonPropertyName("products")] public Product[] Products { get; set; }
  }

  public partial class Product
  {
    [JsonPropertyName("id")] public long Id { get; set; }

    [JsonPropertyName("title")] public string Title { get; set; }

    [JsonPropertyName("handle")] public string Handle { get; set; }

    [JsonPropertyName("body_html")] public string BodyHtml { get; set; }

    [JsonPropertyName("published_at")] public DateTimeOffset PublishedAt { get; set; }

    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("vendor")] public string Vendor { get; set; }

    [JsonPropertyName("product_type")] public string ProductType { get; set; }

    [JsonPropertyName("tags")] public object[] Tags { get; set; }

    [JsonPropertyName("variants")] public Variant[] Variants { get; set; }

    [JsonPropertyName("images")] public Image[] Images { get; set; }

    [JsonPropertyName("options")] public Option[] Options { get; set; }
  }

  public partial class Image
  {
    [JsonPropertyName("id")] public long Id { get; set; }

    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("position")] public long Position { get; set; }

    [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; set; }

    [JsonPropertyName("product_id")] public long ProductId { get; set; }

    [JsonPropertyName("variant_ids")] public object[] VariantIds { get; set; }

    [JsonPropertyName("src")] public string Src { get; set; }

    [JsonPropertyName("width")] public long Width { get; set; }

    [JsonPropertyName("height")] public long Height { get; set; }
  }

  public partial class Option
  {
    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("position")] public long Position { get; set; }

    [JsonPropertyName("values")] public string[] Values { get; set; }
  }

  public partial class Variant
  {
    [JsonPropertyName("id")] public long Id { get; set; }

    [JsonPropertyName("title")] public string Title { get; set; }

    [JsonPropertyName("option1")] public string Option1 { get; set; }

    [JsonPropertyName("option2")] public object Option2 { get; set; }

    [JsonPropertyName("option3")] public object Option3 { get; set; }

    [JsonPropertyName("sku")] public string Sku { get; set; }

    [JsonPropertyName("requires_shipping")]
    public bool RequiresShipping { get; set; }

    [JsonPropertyName("taxable")] public bool Taxable { get; set; }

    [JsonPropertyName("featured_image")] public object FeaturedImage { get; set; }

    [JsonPropertyName("available")] public bool Available { get; set; }

    [JsonPropertyName("price")] public string Price { get; set; }

    [JsonPropertyName("grams")] public long Grams { get; set; }

    [JsonPropertyName("compare_at_price")] public object CompareAtPrice { get; set; }

    [JsonPropertyName("position")] public long Position { get; set; }

    [JsonPropertyName("product_id")] public long ProductId { get; set; }

    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("updated_at")] public DateTimeOffset UpdatedAt { get; set; }
  }
}