using System;
using elasticsearch_nest_webapi_angularjs.Models;
using Nest;

namespace elasticsearch_nest_webapi_angularjs.Services
{
    public class ElasticSearchService : ISearchService<Post>
    {
        private readonly IElasticClient client;

        public ElasticSearchService()
        {
            var node = new Uri("http://localhost:9200");
            var settings = new ConnectionSettings(node);
            settings.DefaultIndex("stack");
            client = new ElasticClient(settings);
        }

        public SearchResult<Post> Search(string query, int page, int pageSize)
        {
            var result = client.Search<Post>(x => x.Query(q => q
                                                        .MultiMatch(mp => mp
                                                            .Query(query)
                                                                .Fields(f => f
                                                                    .Fields(f1 => f1
                                                                        .Title, f2 => f2
                                                                        .Body, f3 => f3
                                                                        .Tags))))
                                                    .From(page - 1)
                                                    .Size(pageSize));

            return new SearchResult<Post>
            {
                Total = (int)result.Total,
                Page = page,
                Results = result.Documents,
                ElapsedMilliseconds = result.Took
            };
        }
    }
}