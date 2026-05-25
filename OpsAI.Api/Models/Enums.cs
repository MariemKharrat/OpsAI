namespace OpsAI.Api.Models;

public enum TicketStatus
{
    New,
    Open,
    InProgress,
    Pending,
    Resolved,
    Closed
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum TicketCategory
{
    Hardware,
    Software,
    Network,
    Security,
    AccessPermissions,
    Email,
    Printing,
    VPN,
    AccountManagement,
    Other
}

public enum SentimentType
{
    Positive,
    Neutral,
    Frustrated,
    Urgent
}
