using OpsAI.Api.Data;
using OpsAI.Api.Models;

namespace OpsAI.Api.Services;

/// <summary>
/// Mock knowledge search service simulating Azure AI Search.
/// </summary>
public class MockKnowledgeSearchService : IKnowledgeSearchService
{
    private readonly CosmosDbService _db;

    public MockKnowledgeSearchService(CosmosDbService db)
    {
        _db = db;
    }

    public async Task<List<KnowledgeArticle>> SearchAsync(string query, int maxResults = 5)
    {
        var articles = await _db.SearchArticlesAsync(query);
        return articles.Take(maxResults).ToList();
    }

    public async Task<List<KnowledgeArticle>> FindRelatedAsync(Ticket ticket, int maxResults = 3)
    {
        // Simulate semantic search by keyword matching
        var searchTerms = ExtractSearchTerms(ticket);
        var allResults = new List<KnowledgeArticle>();

        foreach (var term in searchTerms)
        {
            var results = await _db.SearchArticlesAsync(term);
            allResults.AddRange(results);
        }

        // Also try category match
        var categoryResults = await _db.GetArticlesAsync(ticket.Category);
        allResults.AddRange(categoryResults);

        return allResults
            .DistinctBy(a => a.Id)
            .OrderByDescending(a => a.HelpfulCount)
            .Take(maxResults)
            .ToList();
    }

    private static List<string> ExtractSearchTerms(Ticket ticket)
    {
        var text = $"{ticket.Subject} {ticket.Description}".ToLower();
        var terms = new List<string>();

        var keywords = new Dictionary<string, string>
        {
            ["vpn"] = "vpn",
            ["password"] = "password",
            ["outlook"] = "outlook",
            ["email"] = "email",
            ["printer"] = "printer",
            ["wifi"] = "wifi",
            ["phishing"] = "phishing",
            ["mfa"] = "mfa",
            ["authenticator"] = "authenticator",
            ["access"] = "access",
            ["permission"] = "permission",
            ["monitor"] = "monitor",
            ["docking"] = "docking",
            ["battery"] = "battery",
            ["bitlocker"] = "bitlocker",
            ["software"] = "software",
            ["install"] = "install",
            ["teams"] = "teams",
            ["onboarding"] = "onboarding"
        };

        foreach (var (key, value) in keywords)
        {
            if (text.Contains(key)) terms.Add(value);
        }

        return terms.Take(3).ToList();
    }
}
