using OpsAI.Api.Models;

namespace OpsAI.Api.Services;

public interface IAiTriageService
{
    Task<TriageResult> TriageTicketAsync(Ticket ticket);
}

public interface IAiResolutionService
{
    Task<List<string>> SuggestResolutionStepsAsync(Ticket ticket, List<KnowledgeArticle> relatedArticles);
    Task<string> DraftResponseAsync(Ticket ticket, List<string> resolutionSteps);
    Task<double> PredictSatisfactionAsync(Ticket ticket, string resolution);
}

public interface IKnowledgeSearchService
{
    Task<List<KnowledgeArticle>> SearchAsync(string query, int maxResults = 5);
    Task<List<KnowledgeArticle>> FindRelatedAsync(Ticket ticket, int maxResults = 3);
}
