using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using OpsAI.Api.Data;
using OpsAI.Api.Models;

namespace OpsAI.Api.Services;

public class AzureOpenAiResolutionService : IAiResolutionService
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<AzureOpenAiResolutionService> _logger;

    public AzureOpenAiResolutionService(ChatClient chatClient, ILogger<AzureOpenAiResolutionService> logger)
    {
        _chatClient = chatClient;
        _logger = logger;
    }

    public async Task<List<string>> SuggestResolutionStepsAsync(Ticket ticket, List<KnowledgeArticle> relatedArticles)
    {
        var kbContext = relatedArticles.Any()
            ? "Relevant Knowledge Base Articles:\n" + string.Join("\n---\n", relatedArticles.Select(a => $"Title: {a.Title}\nContent: {a.Content}"))
            : "No directly related knowledge base articles found.";

        var systemPrompt = """
            You are an IT support resolution assistant. Given a ticket and related knowledge base articles, 
            provide clear, actionable troubleshooting steps. Return ONLY a JSON array of strings, each being one step.
            Keep steps concise and technical. Include 4-7 steps. No markdown, no code fences.
            Example: ["Step 1 description","Step 2 description","Step 3 description"]
            """;

        var userMessage = $"""
            Ticket Subject: {ticket.Subject}
            Ticket Description: {ticket.Description}
            Category: {ticket.Category}
            Priority: {ticket.Priority}
            
            {kbContext}
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

            var steps = System.Text.Json.JsonSerializer.Deserialize<List<string>>(content);
            return steps ?? ["Unable to parse resolution steps. Please review the ticket manually."];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure OpenAI resolution suggestion failed");
            var fallback = new MockAiResolutionService();
            return await fallback.SuggestResolutionStepsAsync(ticket, relatedArticles);
        }
    }

    public async Task<string> DraftResponseAsync(Ticket ticket, List<string> resolutionSteps)
    {
        var stepsText = string.Join("\n", resolutionSteps.Select((s, i) => $"{i + 1}. {s}"));

        var systemPrompt = """
            You are an IT support agent drafting a response to an end user. Write a professional, 
            empathetic response that:
            - Acknowledges their issue
            - Provides the resolution steps clearly
            - Includes estimated resolution time based on priority
            - Offers further help if needed
            
            Keep it concise but warm. Return plain text only (no JSON, no markdown formatting).
            """;

        var userMessage = $"""
            Ticket Subject: {ticket.Subject}
            Ticket Description: {ticket.Description}
            Requester Name: {ticket.RequesterName}
            Priority: {ticket.Priority}
            
            Resolution Steps:
            {stepsText}
            """;

        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userMessage)
            };

            var response = await _chatClient.CompleteChatAsync(messages);
            return response.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure OpenAI draft response failed");
            var fallback = new MockAiResolutionService();
            return await fallback.DraftResponseAsync(ticket, resolutionSteps);
        }
    }

    public async Task<double> PredictSatisfactionAsync(Ticket ticket, string resolution)
    {
        var systemPrompt = """
            You are analyzing IT ticket resolution quality. Based on the ticket details and resolution provided,
            predict a customer satisfaction score between 0.0 and 1.0.
            Consider: response time relative to priority, resolution completeness, communication quality.
            Return ONLY a single decimal number (e.g., 0.85). Nothing else.
            """;

        var userMessage = $"""
            Ticket: {ticket.Subject}
            Priority: {ticket.Priority}
            Time open: {(DateTime.UtcNow - ticket.CreatedAt).TotalHours:F1} hours
            Resolution: {resolution}
            """;

        try
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userMessage)
            };

            var response = await _chatClient.CompleteChatAsync(messages);
            var content = response.Value.Content[0].Text.Trim();

            if (double.TryParse(content, out var score))
                return Math.Clamp(score, 0.0, 1.0);

            return 0.82; // Default fallback
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure OpenAI satisfaction prediction failed");
            return 0.82;
        }
    }
}
