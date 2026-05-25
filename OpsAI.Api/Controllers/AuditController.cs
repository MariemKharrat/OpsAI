using Microsoft.AspNetCore.Mvc;
using OpsAI.Api.Data;
using OpsAI.Api.Models;

namespace OpsAI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly CosmosDbService _db;

    public AuditController(CosmosDbService db)
    {
        _db = db;
    }

    [HttpGet("{ticketId}")]
    public async Task<ActionResult<List<AuditEntry>>> GetAuditTrail(string ticketId)
    {
        var entries = await _db.GetAuditEntriesByTicketIdAsync(ticketId);
        return Ok(entries);
    }
}
