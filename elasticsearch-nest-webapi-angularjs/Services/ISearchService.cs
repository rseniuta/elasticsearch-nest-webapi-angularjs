using System.Collections.Generic;
using elasticsearch_nest_webapi_angularjs.Models;

namespace elasticsearch_nest_webapi_angularjs.Services
{
    public interface ISearchService<T>
    {
        SearchResult<T> Search(string query, int page = 1, int pageSize = 10);

        IEnumerable<string> Autocomplete(string query);

        IEnumerable<string> Suggest(string query);

        SearchResult<T> FindMoreLikeThis(string query, int pageSize);

        SearchResult<Post> SearchByCategory(string query, IEnumerable<string> tags, int page, int pageSize);

        T Get(string id);
    }
}