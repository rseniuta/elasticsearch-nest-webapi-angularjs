namespace ElasticsearchWebApi.Core.Services;

public class ElasticOptions
{
    public const string SectionName = "Elasticsearch";

    public string Uri { get; set; } = "http://localhost:9200";

    public string IndexName { get; set; } = "posts";

    public string? Username { get; set; }
        = null;

    public string? Password { get; set; }
        = null;

    public string? CertificateFingerprint { get; set; }
        = null;

    public bool DisableDirectStreaming { get; set; }
        = false;
}
