using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace MonitorsPanel.Web.ManagerApi.Infra
{
  public static class DbContextExtensions
  {
    // ReSharper disable once CognitiveComplexity
    public static void UseSnakeCaseNamingConvention(this ModelBuilder modelBuilder)
    {
      foreach (var entity in modelBuilder.Model.GetEntityTypes())
      {
        // Replace table names
        if (string.IsNullOrEmpty(entity.BaseType?.GetRootType().ShortName()))
        {
          entity.SetTableName(entity.GetTableName().ToSnakeCase());
          entity.SetSchema(entity.GetSchema().ToSnakeCase());
        }

        // Replace column names            
        foreach (var property in entity.GetProperties())
        {
          if (property.IsShadowProperty()) continue;

          property.SetColumnName(property.GetColumnName().ToSnakeCase());
        }

        foreach (var key in entity.GetKeys())
        {
          key.SetName(key.GetName().ToSnakeCase());
        }

        foreach (var key in entity.GetForeignKeys())
        {
          key.SetConstraintName(key.GetDefaultName().ToSnakeCase());
        }

        foreach (var index in entity.GetIndexes())
        {
          index.SetName(index.GetName().ToSnakeCase());
        }
      }
    }
    public static string ToSnakeCase(this string input)
    {
      if (string.IsNullOrEmpty(input))
      {
        return input;
      }

      var startUnderscores = Regex.Match(input, @"^_+");
      return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
    }
    public static string ToCamelCase(this string input)
    {
      if (string.IsNullOrEmpty(input) || !char.IsUpper(input[0]))
      {
        return input;
      }

      // ReSharper disable once RedundantToStringCallForValueType
      return char.ToLowerInvariant(input[0]).ToString() + input.Substring(1);
    }
  }
}