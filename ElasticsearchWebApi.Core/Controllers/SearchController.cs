using ElasticsearchWebApi.Core.Models;
using ElasticsearchWebApi.Core.Models.Requests;
using ElasticsearchWebApi.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace ElasticsearchWebApi.Core.Controllers;

[ApiController]
[Route("api")]
public class SearchController : ControllerBase
{
    private readonly ISearchService<Post> _searchService;
    private readonly ElasticIndexService _indexService;

    public SearchController(ISearchService<Post> searchService, ElasticIndexService indexService)
    {
        _searchService = searchService;
        _indexService = indexService;
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(SearchResult<Post>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery(Name = "q")] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Query parameter 'q' is required.");
        }

        var result = await _searchService.SearchAsync(query, page, pageSize);
        return Ok(result);
    }

    [HttpGet("index")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Index([FromQuery] string fileName, [FromQuery] int maxItems = 1000)
    {
        await _indexService.CreateIndexAsync(fileName, maxItems);
        return Ok();
    }

    [HttpGet("autocomplete")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Autocomplete([FromQuery(Name = "q")] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Query parameter 'q' is required.");
        }

        var suggestions = await _searchService.AutocompleteAsync(query);
        return Ok(suggestions);
    }

    [HttpGet("suggest")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Suggest([FromQuery(Name = "q")] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Query parameter 'q' is required.");
        }

        var suggestions = await _searchService.SuggestAsync(query);
        return Ok(suggestions);
    }

    [HttpGet("morelikethis")]
    [ProducesResponseType(typeof(SearchResult<Post>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MoreLikeThis([FromQuery] string id, [FromQuery] int pageSize = 3)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Parameter 'id' is required.");
        }

        var result = await _searchService.FindMoreLikeThisAsync(id, pageSize);
        return Ok(result);
    }

    [HttpPost("searchbycategory")]
    [ProducesResponseType(typeof(SearchResult<Post>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchByCategory([FromBody] CategorySearchRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _searchService.SearchByCategoryAsync(request.Query, request.Categories, request.Page, request.PageSize);
        return Ok(result);
    }

    [HttpGet("get")]
    [ProducesResponseType(typeof(Post), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromQuery] string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return BadRequest("Parameter 'id' is required.");
        }

        var result = await _searchService.GetAsync(id);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
