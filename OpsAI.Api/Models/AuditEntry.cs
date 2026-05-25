using System.Text.Json.Serialization;

namespace OpsAI.Api.Models;

public class AuditEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("ticketId")]
    public string TicketId { get; set; } = string.Empty;

    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("actor")]
    public string Actor { get; set; } = string.Empty;

    [JsonPropertyName("details")]
    public string Details { get; set; } = string.Empty;

    [JsonPropertyName("previousValue")]
    public string? PreviousValue { get; set; }

    [JsonPropertyName("newValue")]
    public string? NewValue { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; } = "audit";
}
