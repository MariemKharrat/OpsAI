using Microsoft.Azure.Cosmos;
using OpsAI.Api.Models;

namespace OpsAI.Api.Data;

public class CosmosDbService
{
    private readonly CosmosClient _client;
    private readonly Database _database;
    private readonly Container _ticketsContainer;
    private readonly Container _knowledgeContainer;
    private readonly Container _triageContainer;
    private readonly Container _resolutionsContainer;
    private readonly Container _auditContainer;

    public CosmosDbService(CosmosClient client, IConfiguration config)
    {
        _client = client;
        var dbName = config["CosmosDb:DatabaseName"] ?? "OpsAI";
        _database = _client.GetDatabase(dbName);
        _ticketsContainer = _database.GetContainer("Tickets");
        _knowledgeContainer = _database.GetContainer("KnowledgeArticles");
        _triageContainer = _database.GetContainer("TriageResults");
        _resolutionsContainer = _database.GetContainer("Resolutions");
        _auditContainer = _database.GetContainer("AuditEntries");
    }

    public static async Task InitializeDatabaseAsync(CosmosClient client, string databaseName)
    {
        var dbResponse = await client.CreateDatabaseIfNotExistsAsync(databaseName);
        var db = dbResponse.Database;

        await db.CreateContainerIfNotExistsAsync("Tickets", "/partitionKey");
        await db.CreateContainerIfNotExistsAsync("KnowledgeArticles", "/partitionKey");
        await db.CreateContainerIfNotExistsAsync("TriageResults", "/partitionKey");
        await db.CreateContainerIfNotExistsAsync("Resolutions", "/partitionKey");
        await db.CreateContainerIfNotExistsAsync("AuditEntries", "/partitionKey");
    }

    // Tickets
    public async Task<List<Ticket>> GetTicketsAsync(TicketStatus? status = null, TicketPriority? priority = null, 
        TicketCategory? category = null, int page = 1, int pageSize = 20)
    {
        var conditions = new List<string> { "1=1" };
        if (status.HasValue) conditions.Add($"c.status = {(int)status.Value}");
        if (priority.HasValue) conditions.Add($"c.priority = {(int)priority.Value}");
        if (category.HasValue) conditions.Add($"c.category = {(int)category.Value}");

        var where = string.Join(" AND ", conditions);
        var query = new QueryDefinition(
            $"SELECT * FROM c WHERE {where} ORDER BY c.createdAt DESC OFFSET @offset LIMIT @limit")
            .WithParameter("@offset", (page - 1) * pageSize)
            .WithParameter("@limit", pageSize);

        return await QueryAsync<Ticket>(_ticketsContainer, query);
    }

    public async Task<int> GetTicketCountAsync(TicketStatus? status = null, TicketPriority? priority = null, TicketCategory? category = null)
    {
        var conditions = new List<string> { "1=1" };
        if (status.HasValue) conditions.Add($"c.status = {(int)status.Value}");
        if (priority.HasValue) conditions.Add($"c.priority = {(int)priority.Value}");
        if (category.HasValue) conditions.Add($"c.category = {(int)category.Value}");

        var where = string.Join(" AND ", conditions);
        var query = new QueryDefinition($"SELECT VALUE COUNT(1) FROM c WHERE {where}");

        var iterator = _ticketsContainer.GetItemQueryIterator<int>(query);
        var response = await iterator.ReadNextAsync();
        return response.FirstOrDefault();
    }

    public async Task<Ticket?> GetTicketAsync(string id)
    {
        try
        {
            var response = await _ticketsContainer.ReadItemAsync<Ticket>(id, new PartitionKey("ticket"));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Ticket> CreateTicketAsync(Ticket ticket)
    {
        var response = await _ticketsContainer.CreateItemAsync(ticket, new PartitionKey(ticket.PartitionKey));
        return response.Resource;
    }

    public async Task<Ticket> UpdateTicketAsync(Ticket ticket)
    {
        ticket.UpdatedAt = DateTime.UtcNow;
        var response = await _ticketsContainer.ReplaceItemAsync(ticket, ticket.Id, new PartitionKey(ticket.PartitionKey));
        return response.Resource;
    }

    public async Task DeleteTicketAsync(string id)
    {
        await _ticketsContainer.DeleteItemAsync<Ticket>(id, new PartitionKey("ticket"));
    }

    public async Task<List<Ticket>> GetAllTicketsAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c ORDER BY c.createdAt DESC");
        return await QueryAsync<Ticket>(_ticketsContainer, query);
    }

    // Knowledge Articles
    public async Task<List<KnowledgeArticle>> GetArticlesAsync(TicketCategory? category = null)
    {
        var sql = category.HasValue
            ? $"SELECT * FROM c WHERE c.category = {(int)category.Value} ORDER BY c.helpfulCount DESC"
            : "SELECT * FROM c ORDER BY c.helpfulCount DESC";
        return await QueryAsync<KnowledgeArticle>(_knowledgeContainer, new QueryDefinition(sql));
    }

    public async Task<KnowledgeArticle?> GetArticleAsync(string id)
    {
        try
        {
            var response = await _knowledgeContainer.ReadItemAsync<KnowledgeArticle>(id, new PartitionKey("knowledge"));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<List<KnowledgeArticle>> SearchArticlesAsync(string searchTerm)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE CONTAINS(LOWER(c.title), @term) OR CONTAINS(LOWER(c.content), @term)")
            .WithParameter("@term", searchTerm.ToLower());
        return await QueryAsync<KnowledgeArticle>(_knowledgeContainer, query);
    }

    public async Task<KnowledgeArticle> CreateArticleAsync(KnowledgeArticle article)
    {
        var response = await _knowledgeContainer.CreateItemAsync(article, new PartitionKey(article.PartitionKey));
        return response.Resource;
    }

    // Triage Results
    public async Task<TriageResult> CreateTriageResultAsync(TriageResult result)
    {
        var response = await _triageContainer.CreateItemAsync(result, new PartitionKey(result.PartitionKey));
        return response.Resource;
    }

    public async Task<TriageResult?> GetTriageByTicketIdAsync(string ticketId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.ticketId = @ticketId")
            .WithParameter("@ticketId", ticketId);
        var results = await QueryAsync<TriageResult>(_triageContainer, query);
        return results.FirstOrDefault();
    }

    // Resolutions
    public async Task<Resolution> CreateResolutionAsync(Resolution resolution)
    {
        var response = await _resolutionsContainer.CreateItemAsync(resolution, new PartitionKey(resolution.PartitionKey));
        return response.Resource;
    }

    public async Task<Resolution?> GetResolutionByTicketIdAsync(string ticketId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.ticketId = @ticketId")
            .WithParameter("@ticketId", ticketId);
        var results = await QueryAsync<Resolution>(_resolutionsContainer, query);
        return results.FirstOrDefault();
    }

    // Audit Entries
    public async Task<AuditEntry> CreateAuditEntryAsync(AuditEntry entry)
    {
        var response = await _auditContainer.CreateItemAsync(entry, new PartitionKey(entry.PartitionKey));
        return response.Resource;
    }

    public async Task<List<AuditEntry>> GetAuditEntriesByTicketIdAsync(string ticketId)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.ticketId = @ticketId ORDER BY c.timestamp DESC")
            .WithParameter("@ticketId", ticketId);
        return await QueryAsync<AuditEntry>(_auditContainer, query);
    }

    private async Task<List<T>> QueryAsync<T>(Container container, QueryDefinition query)
    {
        var results = new List<T>();
        var iterator = container.GetItemQueryIterator<T>(query);
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }
}
