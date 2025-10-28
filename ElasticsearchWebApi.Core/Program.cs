using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using ElasticsearchWebApi.Core.Models;
using ElasticsearchWebApi.Core.Services;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ElasticOptions>(builder.Configuration.GetSection("Elasticsearch"));

builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<ElasticOptions>>().Value;
    if (string.IsNullOrWhiteSpace(options.Uri))
    {
        throw new InvalidOperationException("Elasticsearch:Uri configuration value is required.");
    }

    var settings = new ElasticsearchClientSettings(new Uri(options.Uri))
        .DefaultIndex(options.IndexName)
        .EnableApiVersioningHeader();

    if (!string.IsNullOrWhiteSpace(options.Username) && !string.IsNullOrWhiteSpace(options.Password))
    {
        settings = settings.Authentication(new BasicAuthentication(options.Username, options.Password));
    }

    if (options.DisableDirectStreaming)
    {
        settings = settings.DisableDirectStreaming();
    }

    if (options.CertificateFingerprint is { Length: > 0 })
    {
        settings = settings.CertificateFingerprint(options.CertificateFingerprint);
    }

    return new ElasticsearchClient(settings);
});

builder.Services.AddSingleton<ElasticIndexService>();
builder.Services.AddScoped<ISearchService<Post>, ElasticSearchService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

namespace ElasticsearchWebApi.Core
{
    public partial class Program
    {
    }
}
