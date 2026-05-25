using Microsoft.AspNetCore.Mvc;
using OpsAI.Api.Data;
using OpsAI.Api.Models;
using OpsAI.Api.Services;

namespace OpsAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KnowledgeController : ControllerBase
{
    private readonly CosmosDbService _db;
    private readonly IKnowledgeSearchService _searchService;

    public KnowledgeController(CosmosDbService db, IKnowledgeSearchService searchService)
    {
        _db = db;
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<ActionResult<List<KnowledgeArticle>>> GetArticles([FromQuery] TicketCategory? category)
    {
        var articles = await _db.GetArticlesAsync(category);
        return Ok(articles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<KnowledgeArticle>> GetArticle(string id)
    {
        var article = await _db.GetArticleAsync(id);
        if (article == null) return NotFound();
        return Ok(article);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<KnowledgeArticle>>> SearchArticles([FromQuery] string q, [FromQuery] int maxResults = 5)
    {
        if (string.IsNullOrWhiteSpace(q)) return BadRequest("Search query is required");
        var results = await _searchService.SearchAsync(q, maxResults);
        return Ok(results);
    }
}
