using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Linq;
using elasticsearch_nest_webapi_angularjs.Models;
using elasticsearch_nest_webapi_angularjs.Utils;
using Nest;

namespace elasticsearch_nest_webapi_angularjs.Services
{
    public class ElasticIndexService
    {
        private readonly IElasticClient client;

        public ElasticIndexService()
        {
            client = ElasticConfig.GetClient();
        }

        public void CreateIndex(string fileName, int maxItems)
        {
            if (!client.IndexExists(ElasticConfig.IndexName).Exists)
            {
                var indexDescriptor = new CreateIndexDescriptor(ElasticConfig.IndexName)
                    .Mappings(ms => ms
                        .Map<Post>(m => m.AutoMap()));

                client.CreateIndex(ElasticConfig.IndexName, i=> indexDescriptor);
            }

            BulkIndex(HostingEnvironment.MapPath("~/data/" + fileName), maxItems);
        }

        private IEnumerable<Post> LoadPostsFromFile(string inputUrl)
        {
            using (XmlReader reader = XmlReader.Create(inputUrl))
            {
                reader.MoveToContent();
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "row")
                    {
                        if (String.Equals(reader.GetAttribute("PostTypeId"), "1"))
                        {
                            XElement el = XNode.ReadFrom(reader) as XElement;

                            if (el != null)
                            {
                                Post post = new Post
                                {
                                    Id = el.Attribute("Id").Value,
                                    Title = el.Attribute("Title") != null ? el.Attribute("Title").Value : "",
                                    CreationDate = DateTime.Parse(el.Attribute("CreationDate").Value),
                                    Score = int.Parse(el.Attribute("Score").Value),
                                    Body = HtmlRemoval.StripTagsRegex(el.Attribute("Body").Value),
                                    Tags =
                                        el.Attribute("Tags") != null
                                            ? el.Attribute("Tags")
                                                .Value.Replace("><", "|")
                                                .Replace("<", "")
                                                .Replace(">", "")
                                                .Replace("&gt;&lt;", "|")
                                                .Replace("&lt;", "")
                                                .Replace("&gt;", "")
                                                .Split('|')
                                            : null,
                                    AnswerCount =
                                        el.Attribute("AnswerCount") != null
                                            ? int.Parse(el.Attribute("AnswerCount").Value)
                                            : 0
                                };
                                post.Suggest = post.Tags;
                                yield return post;
                            }
                        }
                    }
                }
            }
        }

        private void BulkIndex(string path, int maxItems)
        {
            int i = 0;
            int take = maxItems;
            int batch = 1000;
            foreach (var batches in LoadPostsFromFile(path).Take(take).Batch(batch))
            {
                i++;
                var result = client.IndexMany<Post>(batches, "stackoverflow");
            }
        }
    }
}