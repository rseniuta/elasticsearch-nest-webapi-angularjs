using ElasticsearchWebApi.Core.Models;

namespace ElasticsearchWebApi.Core.Services;

public interface ISearchService<T>
{
    Task<SearchResult<T>> SearchAsync(string query, int page, int pageSize);

    Task<SearchResult<T>> SearchByCategoryAsync(string query, IEnumerable<string> tags, int page, int pageSize);

    Task<IEnumerable<string>> AutocompleteAsync(string query);

    Task<IEnumerable<string>> SuggestAsync(string query);

    Task<SearchResult<T>> FindMoreLikeThisAsync(string id, int pageSize);

    Task<T?> GetAsync(string id);
}
