using System.Text.RegularExpressions;

namespace ElasticsearchWebApi.Core.Utils;

public static class EnumerableExtensions
{
    public static IEnumerable<IReadOnlyCollection<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }

        var bucket = new List<TSource>(size);
        foreach (var item in source)
        {
            bucket.Add(item);
            if (bucket.Count == size)
            {
                yield return bucket.ToArray();
                bucket.Clear();
            }
        }

        if (bucket.Count > 0)
        {
            yield return bucket.ToArray();
        }
    }
}

public static class HtmlRemoval
{
    private static readonly Regex HtmlRegex = new("<.*?>", RegexOptions.Compiled);

    public static string StripTagsRegex(string source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return string.Empty;
        }

        return HtmlRegex.Replace(source, string.Empty);
    }
}
