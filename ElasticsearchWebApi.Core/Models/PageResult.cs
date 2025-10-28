using System.Text.Json.Serialization;

namespace ElasticsearchWebApi.Core.Models;

public class PageResult<T>
{
    [JsonPropertyName("total")]
    public int Total { get; set; }
        = 0;

    [JsonPropertyName("page")]
    public int Page { get; set; }
        = 1;

    [JsonPropertyName("data")]
    public IEnumerable<T> Data { get; set; }
        = Array.Empty<T>();
}
