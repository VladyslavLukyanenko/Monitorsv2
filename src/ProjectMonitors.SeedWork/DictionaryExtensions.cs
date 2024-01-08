using System;
using System.Collections.Generic;

namespace ProjectMonitors.SeedWork
{
  public static class DictionaryExtensions
  {
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> self, TKey key,
      Func<TValue> factory)
    {
      return self.GetOrAdd(key, factory, static f => f());
    }

    public static TValue GetOrAdd<TKey, TValue, TArg>(this IDictionary<TKey, TValue> self, TKey key, TArg arg,
      Func<TArg, TValue> factory)
    {
      if (!self.TryGetValue(key, out var value))
      {
        self[key] = value = factory(arg);
      }

      return value;
    }
  }
}