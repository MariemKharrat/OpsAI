using System.Text.Json.Serialization;

namespace OpsAI.Api.Models;

public class Resolution
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("ticketId")]
    public string TicketId { get; set; } = string.Empty;

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("steps")]
    public List<string> Steps { get; set; } = [];

    [JsonPropertyName("draftResponse")]
    public string DraftResponse { get; set; } = string.Empty;

    [JsonPropertyName("resolvedBy")]
    public string ResolvedBy { get; set; } = string.Empty;

    [JsonPropertyName("resolutionType")]
    public string ResolutionType { get; set; } = string.Empty; // "resolved", "escalated", "deferred"

    [JsonPropertyName("timeToResolveMinutes")]
    public int? TimeToResolveMinutes { get; set; }

    [JsonPropertyName("satisfactionPrediction")]
    public double? SatisfactionPrediction { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; } = "resolution";
}
