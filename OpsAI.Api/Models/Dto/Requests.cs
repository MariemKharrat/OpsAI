namespace OpsAI.Api.Models.Dto;

public class CreateTicketRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RequesterName { get; set; } = string.Empty;
    public string RequesterEmail { get; set; } = string.Empty;
    public string RequesterDepartment { get; set; } = string.Empty;
    public TicketCategory? Category { get; set; }
    public TicketPriority? Priority { get; set; }
    public List<string>? Tags { get; set; }
    public bool TriggerAiTriage { get; set; } = true;
}

public class UpdateTicketRequest
{
    public string? Subject { get; set; }
    public string? Description { get; set; }
    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public TicketCategory? Category { get; set; }
    public string? Assignee { get; set; }
    public List<string>? Tags { get; set; }
}

public class TicketListResponse
{
    public List<Ticket> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class DashboardStats
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int ResolvedToday { get; set; }
    public int CriticalTickets { get; set; }
    public double AvgResolutionTimeMinutes { get; set; }
    public Dictionary<string, int> ByStatus { get; set; } = new();
    public Dictionary<string, int> ByPriority { get; set; } = new();
    public Dictionary<string, int> ByCategory { get; set; } = new();
    public List<Ticket> RecentTickets { get; set; } = [];
}
