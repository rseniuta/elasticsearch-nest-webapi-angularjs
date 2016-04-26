using System.Collections.Generic;
using System.Web.Http;
using elasticsearch_nest_webapi_angularjs.Services;

namespace elasticsearch_nest_webapi_angularjs.Controllers
{
    [RoutePrefix("api")]
    public class SearchController : ApiController
    {
        private ElasticSearchService service;

        public SearchController()
        {
            service = new ElasticSearchService();
        }

        [HttpGet]
        [Route("search")]
        public IHttpActionResult Search(string q, int page = 1, int pageSize = 10)
        {
            
            var results = service.Search(q, page, pageSize);
            return Ok(results);
        }

        [HttpGet]
        [Route("index")]
        public IHttpActionResult Index(string fileName, int maxItems = 1000)
        {
            var indexService = new ElasticIndexService();
            indexService.CreateIndex(fileName, maxItems);
            return Ok();
        }

        [HttpGet]
        [Route("autocomplete")]
        public IHttpActionResult Autocomplete(string q)
        {
            return Ok(service.Autocomplete(q));
        }

        [HttpGet]
        [Route("suggest")]
        public IHttpActionResult Suggest(string q)
        {
            return Ok(service.Suggest(q));
        }

        [HttpGet]
        [Route("morelikethis")]
        public IHttpActionResult MoreLikeThis(string id, int pageSize = 3)
        {
            return Ok(service.FindMoreLikeThis(id, pageSize));
        }

        [HttpPost]
        [Route("searchbycategory")]
        public IHttpActionResult SearchByCategory([FromBody]dynamic json)
        {
            string q = json.q;
            var categories = (IEnumerable<string>)json.categories.ToObject<List<string>>();
            return Ok(service.SearchByCategory(q, categories, 1, 10));
        }

        [HttpGet]
        [Route("get")]
        public IHttpActionResult Get(string id)
        {
            return Ok(service.Get(id));
        }
    }
}
