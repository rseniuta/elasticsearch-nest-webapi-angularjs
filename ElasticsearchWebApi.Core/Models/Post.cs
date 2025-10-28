using System.Text.Json.Serialization;

namespace ElasticsearchWebApi.Core.Models;

public class Post
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("creationDate")]
    public DateTime? CreationDate { get; set; }
        = null;

    [JsonPropertyName("score")]
    public int? Score { get; set; }
        = null;

    [JsonPropertyName("answerCount")]
    public int? AnswerCount { get; set; }
        = null;

    [JsonPropertyName("body")]
    public string? Body { get; set; }
        = null;

    [JsonPropertyName("title")]
    public string? Title { get; set; }
        = null;

    [JsonPropertyName("tags")]
    public IEnumerable<string>? Tags { get; set; }
        = null;

    [JsonPropertyName("suggest")]
    public IEnumerable<string>? Suggest { get; set; }
        = null;
}
