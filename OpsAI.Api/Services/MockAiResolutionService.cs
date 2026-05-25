using OpsAI.Api.Data;
using OpsAI.Api.Models;

namespace OpsAI.Api.Services;

/// <summary>
/// Mock resolution service simulating Azure OpenAI GPT-4o suggestions.
/// </summary>
public class MockAiResolutionService : IAiResolutionService
{
    public async Task<List<string>> SuggestResolutionStepsAsync(Ticket ticket, List<KnowledgeArticle> relatedArticles)
    {
        await Task.Delay(400);

        var steps = new List<string>();
        var text = $"{ticket.Subject} {ticket.Description}".ToLower();

        if (text.Contains("vpn"))
        {
            steps.AddRange(["Verify user's internet connectivity is stable",
                "Check VPN client version and update if necessary",
                "Test connectivity to alternate VPN server (vpn2.company.com)",
                "Flush DNS cache and retry connection",
                "If persists, check firewall rules and ISP throttling"]);
        }
        else if (text.Contains("password") || text.Contains("locked"))
        {
            steps.AddRange(["Verify user identity via employee ID and security questions",
                "Check Active Directory for account lock status",
                "Unlock account and initiate password reset",
                "Confirm user can log in with temporary password",
                "Remind user of password policy requirements"]);
        }
        else if (text.Contains("phishing") || text.Contains("suspicious"))
        {
            steps.AddRange(["Quarantine the reported email immediately",
                "Extract email headers and analyze sender domain",
                "Check if other users received similar emails",
                "Block sender domain at mail gateway",
                "Send company-wide awareness notification if campaign confirmed",
                "Check if any users clicked links (review proxy logs)"]);
        }
        else if (text.Contains("monitor") || text.Contains("display") || text.Contains("docking"))
        {
            steps.AddRange(["Check and reseat all cable connections",
                "Try a different video cable (DisplayPort preferred)",
                "Update docking station firmware",
                "Adjust display refresh rate to 60Hz",
                "Test monitor with direct laptop connection to isolate dock issue"]);
        }
        else if (text.Contains("printer") || text.Contains("paper jam"))
        {
            steps.AddRange(["Power off the printer completely",
                "Remove paper tray and check for debris/torn paper",
                "Open rear access panel and clear any jammed paper",
                "Reseat the paper tray and power on",
                "Run test print from printer control panel"]);
        }
        else
        {
            steps.AddRange(["Gather additional information from the requester",
                "Check knowledge base for similar resolved tickets",
                "Attempt standard troubleshooting steps",
                "Escalate to L2 support if unresolved",
                "Document resolution steps for future reference"]);
        }

        // Add relevant KB context
        if (relatedArticles.Any())
        {
            steps.Add($"Reference KB article: '{relatedArticles.First().Title}' for detailed procedure");
        }

        return steps;
    }

    public async Task<string> DraftResponseAsync(Ticket ticket, List<string> resolutionSteps)
    {
        await Task.Delay(300);

        var stepsText = string.Join("\n", resolutionSteps.Select((s, i) => $"{i + 1}. {s}"));

        return $"""
            Hi {ticket.RequesterName.Split(' ')[0]},

            Thank you for reaching out regarding your issue: "{ticket.Subject}".

            I've reviewed your request and here's what I recommend:

            {stepsText}

            I'll be working on this and will update you with progress. If you have any questions or if the issue changes, please don't hesitate to reply to this ticket.

            Expected resolution time: {EstimateResolutionTime(ticket)}

            Best regards,
            IT Support Team
            """;
    }

    public async Task<double> PredictSatisfactionAsync(Ticket ticket, string resolution)
    {
        await Task.Delay(100);

        var baseSatisfaction = ticket.Priority switch
        {
            TicketPriority.Critical => 0.70,
            TicketPriority.High => 0.78,
            TicketPriority.Medium => 0.85,
            TicketPriority.Low => 0.90,
            _ => 0.82
        };

        // Boost if resolved quickly
        if (resolution.Length > 100) baseSatisfaction += 0.05;

        return Math.Min(0.98, Math.Round(baseSatisfaction + Random.Shared.NextDouble() * 0.08, 2));
    }

    private static string EstimateResolutionTime(Ticket ticket)
    {
        return ticket.Priority switch
        {
            TicketPriority.Critical => "Within 1 hour",
            TicketPriority.High => "Within 4 hours",
            TicketPriority.Medium => "1-2 business days",
            TicketPriority.Low => "3-5 business days",
            _ => "2-3 business days"
        };
    }
}
