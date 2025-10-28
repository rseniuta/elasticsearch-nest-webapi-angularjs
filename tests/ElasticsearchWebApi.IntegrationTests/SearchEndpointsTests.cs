using System.Net.Http.Json;
using Elastic.Clients.Elasticsearch;
using ElasticsearchWebApi.Core.Models;
using ElasticsearchWebApi.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;
using Xunit.Sdk;

namespace ElasticsearchWebApi.IntegrationTests;

public class SearchEndpointsTests : IAsyncLifetime
{
    private readonly ElasticsearchWebApplicationFactory _factory = new();
    private ElasticsearchClient? _client;
    private ElasticOptions? _options;

    public async Task InitializeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        _client = scope.ServiceProvider.GetRequiredService<ElasticsearchClient>();
        _options = scope.ServiceProvider.GetRequiredService<IOptions<ElasticOptions>>().Value;

        if (!await IsElasticsearchAvailableAsync(_client))
        {
            throw new SkipException("Elasticsearch instance is not available. Set ELASTICSEARCH_URL to a running 8.x cluster to run integration tests.");
        }

        await CleanupIndexAsync();
        await EnsureTestIndexAsync();
    }

    public async Task DisposeAsync()
    {
        if (_client is not null && _options is not null)
        {
            await CleanupIndexAsync();
        }

        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task SearchEndpoint_ReturnsIndexedDocument()
    {
        Assert.NotNull(_client);
        Assert.NotNull(_options);

        var post = new Post
        {
            Id = Guid.NewGuid().ToString("N"),
            Title = "integration test",
            Body = "integration test body",
            Tags = new[] { "integration", "tests" },
            Suggest = new[] { "integration", "tests" },
            CreationDate = DateTime.UtcNow,
            Score = 5,
            AnswerCount = 1
        };

        var indexResponse = await _client!.IndexAsync(post, i => i.Index(_options!.IndexName).Id(post.Id));
        Assert.True(indexResponse.IsValidResponse, indexResponse.DebugInformation);

        await _client.Indices.RefreshAsync(_options.IndexName);

        var httpClient = _factory.CreateClient();
        var response = await httpClient.GetAsync($"api/search?q={Uri.EscapeDataString("integration")}");
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SearchResult<Post>>();
        Assert.NotNull(result);
        Assert.True(result!.Total >= 1);
        Assert.Contains(result.Results, r => string.Equals(r.Id, post.Id, StringComparison.OrdinalIgnoreCase));
    }

    private async Task EnsureTestIndexAsync()
    {
        if (_client is null || _options is null)
        {
            return;
        }

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

        Assert.True(response.IsValidResponse, response.DebugInformation);
    }

    private async Task CleanupIndexAsync()
    {
        if (_client is null || _options is null)
        {
            return;
        }

        await _client.Indices.DeleteAsync(_options.IndexName, d => d.IgnoreUnavailable(true));
    }

    private static async Task<bool> IsElasticsearchAvailableAsync(ElasticsearchClient client)
    {
        try
        {
            var ping = await client.PingAsync();
            return ping.IsValidResponse;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
