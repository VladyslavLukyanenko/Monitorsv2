using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MonitorsPanel.Core.Manager;

namespace MonitorsPanel.Web.ManagerApi.Infra.EfMappings
{
  public class ImageInfoEfMapping : IEntityTypeConfiguration<ImageInfo>
  {
    public void Configure(EntityTypeBuilder<ImageInfo> builder)
    {
      builder.Property(_ => _.RequiredSpawnParameters)
        .HasConversion(l => ToJson(l), json => FromJson<List<string>>(json))
        .HasColumnType("jsonb");
    }

    private static JsonSerializerSettings JsonSettings { get; } = new JsonSerializerSettings
    {
      //            ContractResolver = new 
      NullValueHandling = NullValueHandling.Ignore,
      ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    private string ToJson(object value) => JsonConvert.SerializeObject(value, JsonSettings);
    private TResult FromJson<TResult>(string json) => JsonConvert.DeserializeObject<TResult>(json, JsonSettings);
  }
}