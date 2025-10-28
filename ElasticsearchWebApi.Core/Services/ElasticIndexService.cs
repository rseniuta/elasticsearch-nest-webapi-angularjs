using Elastic.Clients.Elasticsearch;
using ElasticsearchWebApi.Core.Models;
using ElasticsearchWebApi.Core.Utils;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System.Xml;
using System.Xml.Linq;

namespace ElasticsearchWebApi.Core.Services;

public class ElasticIndexService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ElasticIndexService> _logger;

    public ElasticIndexService(
        ElasticsearchClient client,
        IOptions<ElasticOptions> options,
        IWebHostEnvironment environment,
        ILogger<ElasticIndexService> logger)
    {
        _client = client;
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task CreateIndexAsync(string fileName, int maxItems)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name must be provided.", nameof(fileName));
        }

        await EnsureIndexExistsAsync();

        var dataPath = Path.Combine(_environment.ContentRootPath, "data", fileName);

        if (!File.Exists(dataPath))
        {
            throw new FileNotFoundException($"Could not locate the data file '{fileName}' in the data directory.", dataPath);
        }

        await BulkIndexAsync(dataPath, maxItems <= 0 ? int.MaxValue : maxItems);
    }

    private async Task EnsureIndexExistsAsync()
    {
        var exists = await _client.Indices.ExistsAsync(_options.IndexName);
        if (exists.Exists)
        {
            return;
        }

        var response = await _client.Indices.CreateAsync(_options.IndexName, c => c
            .Mappings(ms => ms
                .Properties<Post>(ps => ps
                    .Keyword(k => k.Field(f => f.Id))
                    .Date(d => d.Field(f => f.CreationDate))
                    .Number(n => n.Field(f => f.Score))
                    .Number(n => n.Field(f => f.AnswerCount))
                    .Text(t => t.Field(f => f.Body))
                    .Text(t => t.Field(f => f.Title))
                    .Keyword(k => k.Field(f => f.Tags))
                    .Completion(co => co.Field(f => f.Suggest))
                )
            )
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to create index '{_options.IndexName}'. Details: {response.DebugInformation}");
        }
    }

    private IEnumerable<Post> LoadPostsFromFile(string inputPath)
    {
        using var reader = XmlReader.Create(inputPath, new XmlReaderSettings { Async = false });
        reader.MoveToContent();
        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element || reader.Name != "row")
            {
                continue;
            }

            if (!string.Equals(reader.GetAttribute("PostTypeId"), "1", StringComparison.Ordinal))
            {
                continue;
            }

            if (XNode.ReadFrom(reader) is not XElement element)
            {
                continue;
            }

            var post = new Post
            {
                Id = element.Attribute("Id")?.Value ?? Guid.NewGuid().ToString("N"),
                Title = element.Attribute("Title")?.Value ?? string.Empty,
                CreationDate = DateTime.TryParse(element.Attribute("CreationDate")?.Value, out var created)
                    ? created
                    : null,
                Score = int.TryParse(element.Attribute("Score")?.Value, out var score)
                    ? score
                    : null,
                Body = HtmlRemoval.StripTagsRegex(element.Attribute("Body")?.Value ?? string.Empty),
                Tags = ParseTags(element.Attribute("Tags")?.Value),
                AnswerCount = int.TryParse(element.Attribute("AnswerCount")?.Value, out var answers)
                    ? answers
                    : null
            };

            post.Suggest = post.Tags?.ToArray();

            yield return post;
        }
    }

    private async Task BulkIndexAsync(string path, int maxItems)
    {
        var indexed = 0;
        foreach (var post in LoadPostsFromFile(path).Take(maxItems))
        {
            var response = await _client.IndexAsync(post, i => i.Index(_options.IndexName).Id(post.Id));
            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Failed to index document {DocumentId}. Debug information: {DebugInformation}", post.Id, response.DebugInformation);
            }
            else
            {
                indexed++;
            }
        }

        _logger.LogInformation("Indexed {Count} documents into '{IndexName}'", indexed, _options.IndexName);
    }

    private static IEnumerable<string>? ParseTags(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var sanitized = value
            .Replace("><", "|", StringComparison.Ordinal)
            .Replace("<", string.Empty, StringComparison.Ordinal)
            .Replace(">", string.Empty, StringComparison.Ordinal)
            .Replace("&gt;&lt;", "|", StringComparison.Ordinal)
            .Replace("&lt;", string.Empty, StringComparison.Ordinal)
            .Replace("&gt;", string.Empty, StringComparison.Ordinal);

        return sanitized.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
    }
}
