using System.Text.Json.Serialization;

namespace OpsAI.Api.Models;

public class TriageResult
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("ticketId")]
    public string TicketId { get; set; } = string.Empty;

    [JsonPropertyName("suggestedPriority")]
    public TicketPriority SuggestedPriority { get; set; }

    [JsonPropertyName("suggestedCategory")]
    public TicketCategory SuggestedCategory { get; set; }

    [JsonPropertyName("sentiment")]
    public SentimentType Sentiment { get; set; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }

    [JsonPropertyName("reasoning")]
    public string Reasoning { get; set; } = string.Empty;

    [JsonPropertyName("suggestedAssignee")]
    public string? SuggestedAssignee { get; set; }

    [JsonPropertyName("matchedArticleIds")]
    public List<string> MatchedArticleIds { get; set; } = [];

    [JsonPropertyName("keyPhrases")]
    public List<string> KeyPhrases { get; set; } = [];

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; } = "triage";
}
