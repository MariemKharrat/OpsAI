using OpsAI.Api.Data;
using OpsAI.Api.Models;

namespace OpsAI.Api.Services;

/// <summary>
/// Mock AI triage service that simulates Azure OpenAI classification.
/// Replace with real Azure OpenAI calls by injecting ChatClient.
/// </summary>
public class MockAiTriageService : IAiTriageService
{
    private readonly CosmosDbService _db;

    public MockAiTriageService(CosmosDbService db)
    {
        _db = db;
    }

    public async Task<TriageResult> TriageTicketAsync(Ticket ticket)
    {
        await Task.Delay(500); // Simulate AI processing time

        var category = ClassifyCategory(ticket);
        var priority = ClassifyPriority(ticket);
        var sentiment = AnalyzeSentiment(ticket);
        var keyPhrases = ExtractKeyPhrases(ticket);

        // Find related KB articles
        var articles = await _db.GetArticlesAsync(category);
        var matchedIds = articles.Take(3).Select(a => a.Id).ToList();

        var result = new TriageResult
        {
            TicketId = ticket.Id,
            SuggestedPriority = priority,
            SuggestedCategory = category,
            Sentiment = sentiment,
            Confidence = CalculateConfidence(ticket),
            Reasoning = GenerateReasoning(ticket, category, priority, sentiment),
            SuggestedAssignee = SuggestAssignee(category, priority),
            MatchedArticleIds = matchedIds,
            KeyPhrases = keyPhrases
        };

        return result;
    }

    private static TicketCategory ClassifyCategory(Ticket ticket)
    {
        var text = $"{ticket.Subject} {ticket.Description}".ToLower();

        if (text.Contains("vpn") || text.Contains("remote access")) return TicketCategory.VPN;
        if (text.Contains("password") || text.Contains("locked out") || text.Contains("account")) return TicketCategory.AccountManagement;
        if (text.Contains("email") || text.Contains("outlook") || text.Contains("exchange")) return TicketCategory.Email;
        if (text.Contains("printer") || text.Contains("print")) return TicketCategory.Printing;
        if (text.Contains("wifi") || text.Contains("network") || text.Contains("internet") || text.Contains("server")) return TicketCategory.Network;
        if (text.Contains("phishing") || text.Contains("security") || text.Contains("mfa") || text.Contains("bitlocker")) return TicketCategory.Security;
        if (text.Contains("access") || text.Contains("permission") || text.Contains("shared drive")) return TicketCategory.AccessPermissions;
        if (text.Contains("laptop") || text.Contains("monitor") || text.Contains("hardware") || text.Contains("docking")) return TicketCategory.Hardware;
        if (text.Contains("install") || text.Contains("software") || text.Contains("update") || text.Contains("license")) return TicketCategory.Software;

        return TicketCategory.Other;
    }

    private static TicketPriority ClassifyPriority(Ticket ticket)
    {
        var text = $"{ticket.Subject} {ticket.Description}".ToLower();

        if (text.Contains("production") || text.Contains("payment") || text.Contains("phishing campaign") ||
            text.Contains("security incident") || text.Contains("all users affected"))
            return TicketPriority.Critical;

        if (text.Contains("urgent") || text.Contains("asap") || text.Contains("deadline today") ||
            text.Contains("blocking") || text.Contains("can't work") || text.Contains("multiple people"))
            return TicketPriority.High;

        if (text.Contains("when you get a chance") || text.Contains("no rush") || text.Contains("future"))
            return TicketPriority.Low;

        return TicketPriority.Medium;
    }

    private static SentimentType AnalyzeSentiment(Ticket ticket)
    {
        var text = $"{ticket.Subject} {ticket.Description}".ToLower();

        if (text.Contains("urgent") || text.Contains("immediately") || text.Contains("asap") || text.Contains("critical"))
            return SentimentType.Urgent;
        if (text.Contains("frustrated") || text.Contains("unacceptable") || text.Contains("been waiting") || text.Contains("still not"))
            return SentimentType.Frustrated;
        if (text.Contains("thank") || text.Contains("appreciate") || text.Contains("please"))
            return SentimentType.Positive;

        return SentimentType.Neutral;
    }

    private static List<string> ExtractKeyPhrases(Ticket ticket)
    {
        var text = $"{ticket.Subject} {ticket.Description}".ToLower();
        var phrases = new List<string>();

        var keywords = new[] { "vpn", "password", "email", "outlook", "printer", "wifi", "network", "security",
            "phishing", "mfa", "access", "permission", "laptop", "monitor", "software", "install",
            "docker", "teams", "sharepoint", "bitlocker", "docking station", "battery" };

        foreach (var kw in keywords)
        {
            if (text.Contains(kw)) phrases.Add(kw);
        }

        return phrases.Take(5).ToList();
    }

    private static double CalculateConfidence(Ticket ticket)
    {
        var descLength = ticket.Description.Length;
        var baseConfidence = descLength switch
        {
            > 200 => 0.92,
            > 100 => 0.85,
            > 50 => 0.75,
            _ => 0.65
        };
        return Math.Round(baseConfidence + Random.Shared.NextDouble() * 0.05, 2);
    }

    private static string GenerateReasoning(Ticket ticket, TicketCategory category, TicketPriority priority, SentimentType sentiment)
    {
        return $"Based on analysis of the ticket content, this appears to be a {category} issue. " +
               $"Priority is suggested as {priority} due to {(priority == TicketPriority.Critical ? "production impact" : priority == TicketPriority.High ? "time sensitivity and business impact" : "standard request nature")}. " +
               $"User sentiment detected as {sentiment}. " +
               $"Key indicators: subject mentions '{ticket.Subject.Split(' ').Take(3).Aggregate((a, b) => $"{a} {b}")}' " +
               $"and description contains {ticket.Description.Split(' ').Length} words with specific technical context.";
    }

    private static string? SuggestAssignee(TicketCategory category, TicketPriority priority)
    {
        if (priority == TicketPriority.Critical) return "Senior On-Call Engineer";

        return category switch
        {
            TicketCategory.Network or TicketCategory.VPN => "Network Team",
            TicketCategory.Security => "Security Team",
            TicketCategory.AccountManagement or TicketCategory.AccessPermissions => "Lisa Park",
            TicketCategory.Email => "Mike Torres",
            TicketCategory.Hardware or TicketCategory.Printing => "Tom Richards",
            TicketCategory.Software => "Tom Richards",
            _ => "Help Desk L1"
        };
    }
}
