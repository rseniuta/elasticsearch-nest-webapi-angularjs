using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using ElasticsearchWebApi.Core.Models;
using Microsoft.Extensions.Options;

namespace ElasticsearchWebApi.Core.Services;

public class ElasticSearchService : ISearchService<Post>
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticOptions _options;
    private readonly ILogger<ElasticSearchService> _logger;

    public ElasticSearchService(
        ElasticsearchClient client,
        IOptions<ElasticOptions> options,
        ILogger<ElasticSearchService> logger)
    {
        _client = client;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<SearchResult<Post>> SearchAsync(string query, int page, int pageSize)
    {
        var response = await _client.SearchAsync<Post>(s => s
            .Index(_options.IndexName)
            .From(Math.Max(page - 1, 0) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .MultiMatch(mm => mm
                    .Query(query)
                    .Fields(new[] { "title", "body", "tags" })
                )
            )
            .Aggregations(a => a
                .Terms("by_tags", t => t
                    .Field(f => f.Tags)
                    .Size(10)
                )
            )
        );

        EnsureValidResponse(response, nameof(SearchAsync));

        return BuildSearchResult(page, pageSize, response);
    }

    public async Task<SearchResult<Post>> SearchByCategoryAsync(string query, IEnumerable<string> tags, int page, int pageSize)
    {
        var tagFilters = tags?.Where(tag => !string.IsNullOrWhiteSpace(tag)).ToArray() ?? Array.Empty<string>();

        var response = await _client.SearchAsync<Post>(s => s
            .Index(_options.IndexName)
            .From(Math.Max(page - 1, 0) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .Bool(b =>
                    b.Must(m => m
                        .MultiMatch(mm => mm
                            .Query(query)
                            .Fields(new[] { "title", "body", "tags" })
                        )
                    )
                    .Filter(f => f
                        .Bool(inner => inner
                            .Must(tagFilters
                                .Select<Func<QueryDescriptor<Post>, Query>>(tag => fq => fq.Term(t => t
                                    .Field(ff => ff.Tags)
                                    .Value(tag)
                                ))
                                .ToArray())
                        )
                    )
                )
            )
            .Aggregations(a => a
                .Terms("by_tags", t => t
                    .Field(f => f.Tags)
                    .Size(10)
                )
            )
        );

        EnsureValidResponse(response, nameof(SearchByCategoryAsync));

        return BuildSearchResult(page, pageSize, response);
    }

    public async Task<IEnumerable<string>> AutocompleteAsync(string query)
    {
        var response = await _client.SearchAsync<Post>(s => s
            .Index(_options.IndexName)
            .Size(0)
            .Suggest(su => su
                .Completion("tag-suggestions", c => c
                    .Field(f => f.Suggest)
                    .Prefix(query)
                    .SkipDuplicates()
                    .Size(6)
                )
            )
        );

        EnsureValidResponse(response, nameof(AutocompleteAsync));

        if (!response.Suggest.TryGetValue("tag-suggestions", out var suggestions))
        {
            return Enumerable.Empty<string>();
        }

        return suggestions
            .SelectMany(s => s.Options)
            .Select(o => o.Text)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<IEnumerable<string>> SuggestAsync(string query)
    {
        var response = await _client.SearchAsync<Post>(s => s
            .Index(_options.IndexName)
            .Size(0)
            .Suggest(su => su
                .Term("post-suggestions", t => t
                    .Text(query)
                    .Field(f => f.Body)
                    .Field(f => f.Title)
                    .Field(f => f.Tags)
                    .Size(6)
                )
            )
        );

        EnsureValidResponse(response, nameof(SuggestAsync));

        if (!response.Suggest.TryGetValue("post-suggestions", out var suggestions))
        {
            return Enumerable.Empty<string>();
        }

        return suggestions
            .SelectMany(s => s.Options)
            .Select(o => o.Text)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<SearchResult<Post>> FindMoreLikeThisAsync(string id, int pageSize)
    {
        var response = await _client.SearchAsync<Post>(s => s
            .Index(_options.IndexName)
            .Size(pageSize)
            .Query(q => q
                .MoreLikeThis(mlt => mlt
                    .Like(l => l.Document(d => d.Id(id)))
                    .Fields(new[] { "title", "body", "tags" })
                    .MinTermFrequency(1)
                )
            )
        );

        EnsureValidResponse(response, nameof(FindMoreLikeThisAsync));

        return BuildSearchResult(1, pageSize, response);
    }

    public async Task<Post?> GetAsync(string id)
    {
        var response = await _client.GetAsync<Post>(id, g => g.Index(_options.IndexName));

        if (!response.IsValidResponse)
        {
            _logger.LogWarning("Failed to fetch document {DocumentId} from Elasticsearch. Reason: {DebugInformation}", id, response.DebugInformation);
            return default;
        }

        return response.Source;
    }

    private void EnsureValidResponse(SearchResponse<Post> response, string operation)
    {
        if (response.IsValidResponse)
        {
            return;
        }

        _logger.LogError("Elasticsearch {Operation} operation failed. Debug information: {DebugInformation}", operation, response.DebugInformation);
        throw new InvalidOperationException($"Elasticsearch {operation} operation failed.");
    }

    private SearchResult<Post> BuildSearchResult(int page, int pageSize, SearchResponse<Post> response)
    {
        var total = response.HitsMetadata?.Total?.Value ?? response.Documents.Count;
        var aggregations = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

        if (response.Aggregations is { } aggs && aggs.TryGetValue("by_tags", out var aggregate))
        {
            switch (aggregate)
            {
                case StringTermsAggregate stringTerms:
                    foreach (var bucket in stringTerms.Buckets)
                    {
                        if (!string.IsNullOrEmpty(bucket.Key))
                        {
                            aggregations[bucket.Key] = bucket.DocCount ?? 0;
                        }
                    }
                    break;
                case TermsAggregate terms:
                    foreach (var bucket in terms.Buckets)
                    {
                        var key = bucket.KeyAsString ?? bucket.Key?.ToString();
                        if (!string.IsNullOrEmpty(key))
                        {
                            aggregations[key] = bucket.DocCount ?? 0;
                        }
                    }
                    break;
            }
        }

        return new SearchResult<Post>
        {
            Total = (int)total,
            Page = page,
            PageSize = pageSize,
            Results = response.Documents.ToArray(),
            ElapsedMilliseconds = response.Took,
            AggregationsByTags = aggregations
        };
    }
}
