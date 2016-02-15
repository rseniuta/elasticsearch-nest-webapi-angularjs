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
            if (!client.IndexExists(Indices.Index(new IndexName() {Name = ElasticConfig.IndexName})).Exists)
            {
                client.CreateIndex(ElasticConfig.IndexName);
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
                                    ViewCount =
                                        el.Attribute("ViewCount") != null
                                            ? int.Parse(el.Attribute("ViewCount").Value)
                                            : 0,
                                    Body = HtmlRemoval.StripTagsRegex(el.Attribute("Body").Value),
                                    LastEditDate =
                                        el.Attribute("LastEditDate") != null
                                            ? (DateTime?)DateTime.Parse(el.Attribute("LastEditDate").Value)
                                            : null,
                                    LastActivityDate =
                                        el.Attribute("LastActivityDate") != null
                                            ? (DateTime?)DateTime.Parse(el.Attribute("LastActivityDate").Value)
                                            : null,
                                    FavoriteCount =
                                        el.Attribute("FavoriteCount") != null
                                            ? int.Parse(el.Attribute("FavoriteCount").Value)
                                            : 0,
                                    CommentCount =
                                        el.Attribute("CommentCount") != null
                                            ? int.Parse(el.Attribute("CommentCount").Value)
                                            : 0,
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
                var result = client.IndexMany<Post>(batches, ElasticConfig.IndexName);
            }
        }
    }
}