using System.Web.Http;
using elasticsearch_nest_webapi_angularjs.Models;
using elasticsearch_nest_webapi_angularjs.Services;

namespace elasticsearch_nest_webapi_angularjs.Controllers
{
    [RoutePrefix("api")]
    public class SearchController : ApiController
    {
        [HttpGet]
        [Route("search")]
        public IHttpActionResult Search(string q, int page = 1, int pageSize = 10)
        {
            ISearchService<Post> service = new ElasticSearchService();
            var results = service.Search(q, page, pageSize);
            return Ok(results);
        }

        [HttpGet]
        [Route("index")]
        public IHttpActionResult Index(string fileName, int maxItems = 1000)
        {
            var service = new ElasticIndexService();
            service.CreateIndex(fileName, maxItems);
            return Ok();
        }
    }
}
