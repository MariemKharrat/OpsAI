using System.Text.Json.Serialization;

namespace OpsAI.Api.Models;

public class Ticket
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("ticketNumber")]
    public string TicketNumber { get; set; } = string.Empty;

    [JsonPropertyName("subject")]
    public string Subject { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public TicketStatus Status { get; set; } = TicketStatus.New;

    [JsonPropertyName("priority")]
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    [JsonPropertyName("category")]
    public TicketCategory Category { get; set; } = TicketCategory.Other;

    [JsonPropertyName("requesterName")]
    public string RequesterName { get; set; } = string.Empty;

    [JsonPropertyName("requesterEmail")]
    public string RequesterEmail { get; set; } = string.Empty;

    [JsonPropertyName("requesterDepartment")]
    public string RequesterDepartment { get; set; } = string.Empty;

    [JsonPropertyName("assignee")]
    public string? Assignee { get; set; }

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = [];

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("resolvedAt")]
    public DateTime? ResolvedAt { get; set; }

    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; } = "ticket";
}
