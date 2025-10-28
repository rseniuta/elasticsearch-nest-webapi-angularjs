using System.Text.Json.Serialization;

namespace ElasticsearchWebApi.Core.Models;

public class SearchResult<T>
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
        = 0;

    [JsonPropertyName("page")]
    public int Page { get; set; }
        = 1;

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
        = 0;

    [JsonPropertyName("results")]
    public IEnumerable<T> Results { get; set; }
        = Array.Empty<T>();

    [JsonPropertyName("elapsedMilliseconds")]
    public long ElapsedMilliseconds { get; set; }
        = 0;

    [JsonPropertyName("aggregationsByTags")]
    public IDictionary<string, long> AggregationsByTags { get; set; }
        = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
}
