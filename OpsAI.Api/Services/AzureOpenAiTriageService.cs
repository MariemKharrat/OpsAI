using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using OpsAI.Api.Data;
using OpsAI.Api.Models;

namespace OpsAI.Api.Services;

public class AzureOpenAiTriageService : IAiTriageService
{
    private readonly ChatClient _chatClient;
    private readonly CosmosDbService _db;
    private readonly ILogger<AzureOpenAiTriageService> _logger;

    public AzureOpenAiTriageService(ChatClient chatClient, CosmosDbService db, ILogger<AzureOpenAiTriageService> logger)
    {
        _chatClient = chatClient;
        _db = db;
        _logger = logger;
    }

    public async Task<TriageResult> TriageTicketAsync(Ticket ticket)
    {
        var systemPrompt = """
            You are an IT support triage AI. Analyze the ticket and respond with ONLY valid JSON (no markdown, no code fences).
            
            Classify the ticket with:
            - suggestedPriority: one of "Low", "Medium", "High", "Critical"
            - suggestedCategory: one of "Hardware", "Software", "Network", "Security", "AccessPermissions", "Email", "Printing", "VPN", "AccountManagement", "Other"
            - sentiment: one of "Positive", "Neutral", "Frustrated", "Urgent"
            - confidence: a number between 0.0 and 1.0
            - reasoning: a brief explanation of your classification
            - suggestedAssignee: suggest a team or person (e.g., "Network Team", "Security Team", "Help Desk L1")
            - keyPhrases: array of up to 5 key technical terms from the ticket
            
            Respond with this exact JSON structure:
            {"suggestedPriority":"...","suggestedCategory":"...","sentiment":"...","confidence":0.0,"reasoning":"...","suggestedAssignee":"...","keyPhrases":["..."]}
            """;

        var userMessage = $"""
            Ticket Subject: {ticket.Subject}
            Ticket Description: {ticket.Description}
            Requester: {ticket.RequesterName} ({ticket.RequesterDepartment})
            """;

        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userMessage)
            };

            var response = await _chatClient.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text;

            // Parse the JSON response
            var json = System.Text.Json.JsonDocument.Parse(content);
            var root = json.RootElement;

            var result = new TriageResult
            {
                TicketId = ticket.Id,
                SuggestedPriority = Enum.Parse<TicketPriority>(root.GetProperty("suggestedPriority").GetString()!),
                SuggestedCategory = Enum.Parse<TicketCategory>(root.GetProperty("suggestedCategory").GetString()!),
                Sentiment = Enum.Parse<SentimentType>(root.GetProperty("sentiment").GetString()!),
                Confidence = root.GetProperty("confidence").GetDouble(),
                Reasoning = root.GetProperty("reasoning").GetString() ?? "",
                SuggestedAssignee = root.GetProperty("suggestedAssignee").GetString(),
                KeyPhrases = root.GetProperty("keyPhrases").EnumerateArray()
                    .Select(e => e.GetString()!).ToList()
            };

            // Find related KB articles
            var articles = await _db.GetArticlesAsync(result.SuggestedCategory);
            result.MatchedArticleIds = articles.Take(3).Select(a => a.Id).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure OpenAI triage failed, falling back to rule-based classification");
            // Fallback to mock if AI fails
            var fallback = new MockAiTriageService(_db);
            return await fallback.TriageTicketAsync(ticket);
        }
    }
}
