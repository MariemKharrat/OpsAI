using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using OpsAI.Api.Data;
using OpsAI.Api.Models;

namespace OpsAI.Api.Services;

public class AzureAiSearchService : IKnowledgeSearchService
{
    private readonly SearchClient? _searchClient;
    private readonly CosmosDbService _db;
    private readonly ILogger<AzureAiSearchService> _logger;
    private readonly bool _isConfigured;

    public AzureAiSearchService(IConfiguration config, CosmosDbService db, ILogger<AzureAiSearchService> logger)
    {
        _db = db;
        _logger = logger;

        var endpoint = config["AzureAISearch:Endpoint"];
        var apiKey = config["AzureAISearch:ApiKey"];
        var indexName = config["AzureAISearch:IndexName"] ?? "opsai-knowledge";

        if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(apiKey))
        {
            try
            {
                _searchClient = new SearchClient(
                    new Uri(endpoint),
                    indexName,
                    new AzureKeyCredential(apiKey));
                _isConfigured = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize Azure AI Search client, falling back to Cosmos DB search");
                _isConfigured = false;
            }
        }
    }

    public async Task<List<KnowledgeArticle>> SearchAsync(string query, int maxResults = 5)
    {
        if (_isConfigured && _searchClient != null)
        {
            try
            {
                var options = new SearchOptions
                {
                    Size = maxResults,
                    QueryType = SearchQueryType.Semantic,
                    SemanticSearch = new SemanticSearchOptions
                    {
                        SemanticConfigurationName = "default"
                    }
                };
                options.Select.Add("id");
                options.Select.Add("title");
                options.Select.Add("content");
                options.Select.Add("category");
                options.Select.Add("tags");

                var response = await _searchClient.SearchAsync<SearchArticleDocument>(query, options);
                var articles = new List<KnowledgeArticle>();

                await foreach (var result in response.Value.GetResultsAsync())
                {
                    articles.Add(new KnowledgeArticle
                    {
                        Id = result.Document.Id,
                        Title = result.Document.Title,
                        Content = result.Document.Content,
                        Tags = result.Document.Tags?.ToList() ?? []
                    });
                }

                if (articles.Count > 0)
                    return articles;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Azure AI Search query failed, falling back to Cosmos DB search");
            }
        }

        // Fallback to Cosmos DB text search
        return await _db.SearchArticlesAsync(query);
    }

    public async Task<List<KnowledgeArticle>> FindRelatedAsync(Ticket ticket, int maxResults = 3)
    {
        // Try semantic search with ticket subject + key terms
        var searchQuery = $"{ticket.Subject} {ExtractKeyTerms(ticket.Description)}";
        var results = await SearchAsync(searchQuery, maxResults);

        if (results.Count > 0)
            return results;

        // Fallback: category-based search from Cosmos
        var categoryResults = await _db.GetArticlesAsync(ticket.Category);
        return categoryResults.Take(maxResults).ToList();
    }

    private static string ExtractKeyTerms(string text)
    {
        // Take first 200 chars for the search query to keep it focused
        return text.Length > 200 ? text[..200] : text;
    }
}

public class SearchArticleDocument
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public int? Category { get; set; }
    public string[]? Tags { get; set; }
}
