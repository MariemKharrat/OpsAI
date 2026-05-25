using Microsoft.AspNetCore.Mvc;
using OpsAI.Api.Data;
using OpsAI.Api.Models;
using OpsAI.Api.Services;

namespace OpsAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly CosmosDbService _db;
    private readonly IAiTriageService _triageService;
    private readonly IAiResolutionService _resolutionService;
    private readonly IKnowledgeSearchService _knowledgeSearch;

    public AiController(
        CosmosDbService db,
        IAiTriageService triageService,
        IAiResolutionService resolutionService,
        IKnowledgeSearchService knowledgeSearch)
    {
        _db = db;
        _triageService = triageService;
        _resolutionService = resolutionService;
        _knowledgeSearch = knowledgeSearch;
    }

    [HttpPost("triage/{ticketId}")]
    public async Task<ActionResult<TriageResult>> TriageTicket(string ticketId)
    {
        var ticket = await _db.GetTicketAsync(ticketId);
        if (ticket == null) return NotFound("Ticket not found");

        var result = await _triageService.TriageTicketAsync(ticket);
        var saved = await _db.CreateTriageResultAsync(result);

        // Create audit entry
        await _db.CreateAuditEntryAsync(new AuditEntry
        {
            TicketId = ticketId,
            Action = "AI Triage",
            Actor = "AI System",
            Details = $"Classified as {result.SuggestedCategory}, {result.SuggestedPriority} priority (confidence: {result.Confidence:P0})"
        });

        return Ok(saved);
    }

    [HttpGet("triage/{ticketId}")]
    public async Task<ActionResult<TriageResult>> GetTriageResult(string ticketId)
    {
        var result = await _db.GetTriageByTicketIdAsync(ticketId);
        if (result == null) return NotFound("No triage result found for this ticket");
        return Ok(result);
    }

    [HttpPost("suggest-resolution/{ticketId}")]
    public async Task<ActionResult<ResolutionSuggestion>> SuggestResolution(string ticketId)
    {
        var ticket = await _db.GetTicketAsync(ticketId);
        if (ticket == null) return NotFound("Ticket not found");

        var relatedArticles = await _knowledgeSearch.FindRelatedAsync(ticket);
        var steps = await _resolutionService.SuggestResolutionStepsAsync(ticket, relatedArticles);

        return Ok(new ResolutionSuggestion
        {
            TicketId = ticketId,
            Steps = steps,
            RelatedArticles = relatedArticles,
            GeneratedAt = DateTime.UtcNow
        });
    }

    [HttpPost("draft-response/{ticketId}")]
    public async Task<ActionResult<DraftResponseResult>> DraftResponse(string ticketId)
    {
        var ticket = await _db.GetTicketAsync(ticketId);
        if (ticket == null) return NotFound("Ticket not found");

        var relatedArticles = await _knowledgeSearch.FindRelatedAsync(ticket);
        var steps = await _resolutionService.SuggestResolutionStepsAsync(ticket, relatedArticles);
        var draft = await _resolutionService.DraftResponseAsync(ticket, steps);

        return Ok(new DraftResponseResult
        {
            TicketId = ticketId,
            DraftResponse = draft,
            ResolutionSteps = steps,
            GeneratedAt = DateTime.UtcNow
        });
    }

    [HttpPost("escalate/{ticketId}")]
    public async Task<ActionResult<EscalationResult>> EscalateTicket(string ticketId, [FromBody] EscalateRequest request)
    {
        var ticket = await _db.GetTicketAsync(ticketId);
        if (ticket == null) return NotFound("Ticket not found");

        // Update ticket
        ticket.Priority = TicketPriority.High;
        ticket.Status = TicketStatus.InProgress;
        ticket.Assignee = request.EscalateTo ?? "Senior Support Engineer";
        await _db.UpdateTicketAsync(ticket);

        // Audit
        await _db.CreateAuditEntryAsync(new AuditEntry
        {
            TicketId = ticketId,
            Action = "Escalated",
            Actor = request.EscalatedBy ?? "IT Support",
            Details = $"Escalated to {ticket.Assignee}. Reason: {request.Reason}",
            PreviousValue = ticket.Priority.ToString(),
            NewValue = "High"
        });

        return Ok(new EscalationResult
        {
            TicketId = ticketId,
            EscalatedTo = ticket.Assignee,
            NewPriority = ticket.Priority,
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpPost("resolve/{ticketId}")]
    public async Task<ActionResult<Resolution>> ResolveTicket(string ticketId, [FromBody] ResolveRequest request)
    {
        var ticket = await _db.GetTicketAsync(ticketId);
        if (ticket == null) return NotFound("Ticket not found");

        var satisfaction = await _resolutionService.PredictSatisfactionAsync(ticket, request.Summary);

        var resolution = new Resolution
        {
            TicketId = ticketId,
            Summary = request.Summary,
            Steps = request.Steps ?? [],
            DraftResponse = request.ResponseSent ?? "",
            ResolvedBy = request.ResolvedBy ?? "IT Support",
            ResolutionType = "resolved",
            TimeToResolveMinutes = (int)(DateTime.UtcNow - ticket.CreatedAt).TotalMinutes,
            SatisfactionPrediction = satisfaction
        };

        var saved = await _db.CreateResolutionAsync(resolution);

        // Update ticket status
        ticket.Status = TicketStatus.Resolved;
        ticket.ResolvedAt = DateTime.UtcNow;
        await _db.UpdateTicketAsync(ticket);

        // Audit
        await _db.CreateAuditEntryAsync(new AuditEntry
        {
            TicketId = ticketId,
            Action = "Resolved",
            Actor = resolution.ResolvedBy,
            Details = resolution.Summary,
            PreviousValue = "InProgress",
            NewValue = "Resolved"
        });

        return Ok(saved);
    }
}

// Response DTOs
public class ResolutionSuggestion
{
    public string TicketId { get; set; } = string.Empty;
    public List<string> Steps { get; set; } = [];
    public List<KnowledgeArticle> RelatedArticles { get; set; } = [];
    public DateTime GeneratedAt { get; set; }
}

public class DraftResponseResult
{
    public string TicketId { get; set; } = string.Empty;
    public string DraftResponse { get; set; } = string.Empty;
    public List<string> ResolutionSteps { get; set; } = [];
    public DateTime GeneratedAt { get; set; }
}

public class EscalateRequest
{
    public string Reason { get; set; } = string.Empty;
    public string? EscalateTo { get; set; }
    public string? EscalatedBy { get; set; }
}

public class EscalationResult
{
    public string TicketId { get; set; } = string.Empty;
    public string EscalatedTo { get; set; } = string.Empty;
    public TicketPriority NewPriority { get; set; }
    public DateTime Timestamp { get; set; }
}

public class ResolveRequest
{
    public string Summary { get; set; } = string.Empty;
    public List<string>? Steps { get; set; }
    public string? ResponseSent { get; set; }
    public string? ResolvedBy { get; set; }
}
