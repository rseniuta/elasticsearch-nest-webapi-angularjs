using Elastic.Clients.Elasticsearch;
using ElasticsearchWebApi.Core;
using ElasticsearchWebApi.Core.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ElasticsearchWebApi.IntegrationTests;

public class ElasticsearchWebApplicationFactory : WebApplicationFactory<Program>
{
    public string IndexName { get; } = $"test-posts-{Guid.NewGuid():N}";

    public Uri ElasticsearchUri { get; }
        = new(Environment.GetEnvironmentVariable("ELASTICSEARCH_URL") ?? "http://localhost:9200");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                [$"{ElasticOptions.SectionName}:Uri"] = ElasticsearchUri.ToString(),
                [$"{ElasticOptions.SectionName}:IndexName"] = IndexName
            };

            configBuilder.AddInMemoryCollection(overrides);
        });
    }
}
