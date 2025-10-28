using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ElasticsearchWebApi.Core.Models.Requests;

public class CategorySearchRequest
{
    [Required]
    [JsonPropertyName("q")]
    public string Query { get; set; } = string.Empty;

    [JsonPropertyName("categories")]
    public IEnumerable<string> Categories { get; set; } = Array.Empty<string>();

    [JsonPropertyName("page")]
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [JsonPropertyName("pageSize")]
    [Range(1, 1000)]
    public int PageSize { get; set; } = 10;
}
