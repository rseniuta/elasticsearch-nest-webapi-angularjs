using System;
using System.Collections.Generic;
using System.Linq;
using elasticsearch_nest_webapi_angularjs.Models;
using Nest;

namespace elasticsearch_nest_webapi_angularjs.Services
{
    public class ElasticSearchService : ISearchService<Post>
    {
        private readonly IElasticClient client;

        public ElasticSearchService()
        {
            client = ElasticConfig.GetClient();
        }

        public SearchResult<Post> Search(string query, int page, int pageSize)
        {
            var result = client.Search<Post>(x => x.Query(q => q
                                                        .MultiMatch(mp => mp
                                                            .Query(query)
                                                                .Fields(f => f
                                                                    .Fields(f1 => f1.Title, f2 => f2.Body, f3 => f3.Tags))))
                                                    .Aggregations(a => a
                                                        .Terms("by_tags", t => t
                                                            .Field(f => f.Tags)
                                                            .Size(10)))
                                                    .From(page - 1)
                                                    .Size(pageSize));

            return new SearchResult<Post>
            {
                Total = (int)result.Total,
                Page = page,
                Results = result.Documents,
                ElapsedMilliseconds = result.Took,
                AggregationsByTags = result.Aggs.Terms("by_tags").Buckets.ToDictionary(x => x.Key, y => y.DocCount.GetValueOrDefault(0))
            };
        }

        public SearchResult<Post> SearchByCategory(string query, IEnumerable<string> tags, int page = 1,
            int pageSize = 10)
        {

            var filters = tags.Select(c => new Func<QueryContainerDescriptor<Post>, QueryContainer>(x => x.Term(f => f.Tags, c))).ToArray();
            
            var result = client.Search<Post>(x => x
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .MultiMatch(mp => mp
                                .Query(query)
                                    .Fields(f => f
                                        .Fields(f1 => f1.Title, f2 => f2.Body, f3 => f3.Tags))))
                        .Filter(f => f
                            .Bool(b1 => b1
                                .Must(filters)))))
                .Aggregations(a => a
                    .Terms("by_tags", t => t
                        .Field(f => f.Tags)
                        .Size(10)
                    )
                )
                .From(page - 1)
                .Size(pageSize));

            return new SearchResult<Post>
            {
                Total = (int)result.Total,
                Page = page,
                Results = result.Documents,
                ElapsedMilliseconds = result.Took,
                AggregationsByTags = result.Aggs.Terms("by_tags").Buckets.ToDictionary(x => x.Key, y => y.DocCount.GetValueOrDefault(0))
            };
        }

        public IEnumerable<string> Autocomplete(string query)
        {
            var result = client.Suggest<Post>(x => x.Completion("tag-suggestions", c => c.Text(query)
                .Field(f => f.Suggest)
                .Size(6)));

            return result.Suggestions["tag-suggestions"].SelectMany(x => x.Options).Select(y => y.Text);
        }

        public IEnumerable<string> Suggest(string query)
        {
            var result = client.Suggest<Post>(x => x.Term("post-suggestions", t => t.Text(query)
                .Field(f => f.Body)
                .Field(f => f.Title)
                .Field(f => f.Tags)
                .Size(6)));

            return result.Suggestions["post-suggestions"].SelectMany(x => x.Options).Select(y => y.Text);
        }

        public SearchResult<Post> FindMoreLikeThis(string id, int pageSize)
        {
            var result = client.Search<Post>(x => x
                .Query(y => y
                    .MoreLikeThis(m => m
                        .Like(l => l.Document(d => d.Id(id)))
                        .Fields(new[] { "title", "body", "tags" })
                        .MinTermFrequency(1)
                        )).Size(pageSize));

            return new SearchResult<Post>
            {
                Total = (int)result.Total,
                Page = 1,
                Results = result.Documents
            };
        }

        public Post Get(string id)
        {
            var result = client.Get<Post>(new DocumentPath<Post>(id));
            return result.Source;
        }
    }
}