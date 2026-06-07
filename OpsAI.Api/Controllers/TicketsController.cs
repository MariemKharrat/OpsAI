using Microsoft.AspNetCore.Mvc;
using OpsAI.Api.Data;
using OpsAI.Api.Models;
using OpsAI.Api.Models.Dto;
using OpsAI.Api.Services;

namespace OpsAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly CosmosDbService _db;
    private readonly IAiTriageService _triageService;

    public TicketsController(CosmosDbService db, IAiTriageService triageService)
    {
        _db = db;
        _triageService = triageService;
    }

    [HttpGet]
    public async Task<ActionResult<TicketListResponse>> GetTickets(
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketPriority? priority,
        [FromQuery] TicketCategory? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var tickets = await _db.GetTicketsAsync(status, priority, category, page, pageSize);
        var total = await _db.GetTicketCountAsync(status, priority, category);

        return Ok(new TicketListResponse
        {
            Items = tickets,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Ticket>> GetTicket(string id)
    {
        var ticket = await _db.GetTicketAsync(id);
        if (ticket == null) return NotFound();
        return Ok(ticket);
    }

    [HttpPost]
    public async Task<ActionResult<Ticket>> CreateTicket([FromBody] CreateTicketRequest request)
    {
        var ticketNumber = $"INC-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(100, 999)}";

        var ticket = new Ticket
        {
            TicketNumber = ticketNumber,
            Subject = request.Subject,
            Description = request.Description,
            RequesterName = request.RequesterName,
            RequesterEmail = request.RequesterEmail,
            RequesterDepartment = request.RequesterDepartment,
            Category = request.Category ?? TicketCategory.Other,
            Priority = request.Priority ?? TicketPriority.Medium,
            Tags = request.Tags ?? []
        };

        var created = await _db.CreateTicketAsync(ticket);

        // Audit entry
        await _db.CreateAuditEntryAsync(new AuditEntry
        {
            TicketId = created.Id,
            Action = "Created",
            Actor = request.RequesterName,
            Details = "Ticket created via portal"
        });

        return CreatedAtAction(nameof(GetTicket), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Ticket>> UpdateTicket(string id, [FromBody] UpdateTicketRequest request)
    {
        var ticket = await _db.GetTicketAsync(id);
        if (ticket == null) return NotFound();

        var changes = new List<string>();

        if (request.Subject != null) { ticket.Subject = request.Subject; changes.Add("subject"); }
        if (request.Description != null) { ticket.Description = request.Description; changes.Add("description"); }
        if (request.Status.HasValue)
        {
            var prev = ticket.Status;
            ticket.Status = request.Status.Value;
            changes.Add($"status: {prev} → {request.Status.Value}");
            if (request.Status.Value == TicketStatus.Resolved)
                ticket.ResolvedAt = DateTime.UtcNow;
        }
        if (request.Priority.HasValue)
        {
            var prev = ticket.Priority;
            ticket.Priority = request.Priority.Value;
            changes.Add($"priority: {prev} → {request.Priority.Value}");
        }
        if (request.Category.HasValue) { ticket.Category = request.Category.Value; changes.Add("category"); }
        if (request.Assignee != null) { ticket.Assignee = request.Assignee; changes.Add($"assignee: {request.Assignee}"); }
        if (request.Tags != null) { ticket.Tags = request.Tags; changes.Add("tags"); }

        var updated = await _db.UpdateTicketAsync(ticket);

        await _db.CreateAuditEntryAsync(new AuditEntry
        {
            TicketId = id,
            Action = "Updated",
            Actor = "IT Support",
            Details = $"Updated: {string.Join(", ", changes)}"
        });

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicket(string id)
    {
        var ticket = await _db.GetTicketAsync(id);
        if (ticket == null) return NotFound();

        await _db.DeleteTicketAsync(id);
        return NoContent();
    }

    [HttpGet("{id}/similar-resolved")]
    public async Task<ActionResult<List<SimilarResolvedTicket>>> GetSimilarResolved(string id)
    {
        var ticket = await _db.GetTicketAsync(id);
        if (ticket == null) return NotFound();

        var resolved = await _db.GetTicketsAsync(
            status: TicketStatus.Resolved,
            category: ticket.Category,
            page: 1,
            pageSize: 10);

        // Exclude the current ticket and limit to 3
        var similar = resolved
            .Where(t => t.Id != id)
            .Take(3)
            .ToList();

        var results = new List<SimilarResolvedTicket>();
        foreach (var t in similar)
        {
            var resolution = await _db.GetResolutionByTicketIdAsync(t.Id);
            results.Add(new SimilarResolvedTicket
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                Subject = t.Subject,
                Category = t.Category,
                ResolvedAt = t.ResolvedAt,
                ResolutionSummary = resolution?.Summary ?? "No resolution note available",
                ResolutionSteps = resolution?.Steps ?? []
            });
        }

        return Ok(results);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStats>> GetStats()
    {
        var allTickets = await _db.GetAllTicketsAsync();
        var today = DateTime.UtcNow.Date;

        var stats = new DashboardStats
        {
            TotalTickets = allTickets.Count,
            OpenTickets = allTickets.Count(t => t.Status is TicketStatus.Open or TicketStatus.InProgress or TicketStatus.New),
            ResolvedToday = allTickets.Count(t => t.ResolvedAt?.Date == today),
            CriticalTickets = allTickets.Count(t => t.Priority == TicketPriority.Critical && t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed),
            AvgResolutionTimeMinutes = allTickets
                .Where(t => t.ResolvedAt.HasValue)
                .Select(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalMinutes)
                .DefaultIfEmpty(0)
                .Average(),
            ByStatus = allTickets.GroupBy(t => t.Status.ToString()).ToDictionary(g => g.Key, g => g.Count()),
            ByPriority = allTickets.GroupBy(t => t.Priority.ToString()).ToDictionary(g => g.Key, g => g.Count()),
            ByCategory = allTickets.GroupBy(t => t.Category.ToString()).ToDictionary(g => g.Key, g => g.Count()),
            RecentTickets = allTickets.Take(5).ToList()
        };

        return Ok(stats);
    }
}
