using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Decorator.DataAccess;

public static class EnumerableExtensions
{
    public static IEnumerable<TSource> Exclude<TSource, TKey>(this IEnumerable<TSource> source,
    IEnumerable<TSource> exclude, Func<TSource, TKey> keySelector)
    {
        var excludedSet = new HashSet<TKey>(exclude.Select(keySelector));
        return source.Where(item => !excludedSet.Contains(keySelector(item)));
    }
}