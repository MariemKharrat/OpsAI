using System.Text.Json;
using System.Text.Json.Serialization;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Azure.Cosmos;
using OpenAI.Chat;
using OpsAI.Api.Data;
using OpsAI.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// JSON serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddOpenApi();

// CORS for React dev server
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Cosmos DB
var cosmosConnectionString = builder.Configuration["CosmosDb:ConnectionString"]
    ?? "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
var cosmosDbName = builder.Configuration["CosmosDb:DatabaseName"] ?? "OpsAI";

var cosmosClient = new CosmosClient(cosmosConnectionString, new CosmosClientOptions
{
    SerializerOptions = new CosmosSerializationOptions
    {
        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
    }
});

builder.Services.AddSingleton(cosmosClient);
builder.Services.AddSingleton<CosmosDbService>();

// Azure OpenAI - ChatClient for GPT-4o
var openAiEndpoint = builder.Configuration["AzureOpenAI:Endpoint"] ?? "";
var openAiKey = builder.Configuration["AzureOpenAI:ApiKey"] ?? "";
var openAiDeployment = builder.Configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4o";

if (!string.IsNullOrEmpty(openAiEndpoint) && !string.IsNullOrEmpty(openAiKey))
{
    // For AI Foundry endpoints (*.services.ai.azure.com), use the base URL
    var endpointUri = new Uri(openAiEndpoint);

    var azureOpenAiClient = new AzureOpenAIClient(
        endpointUri,
        new AzureKeyCredential(openAiKey));
    var chatClient = azureOpenAiClient.GetChatClient(openAiDeployment);
    builder.Services.AddSingleton(chatClient);

    // Real Azure AI Services
    builder.Services.AddSingleton<IAiTriageService, AzureOpenAiTriageService>();
    builder.Services.AddSingleton<IAiResolutionService, AzureOpenAiResolutionService>();
}
else
{
    // Fallback to mock implementations
    builder.Services.AddSingleton<IAiTriageService, MockAiTriageService>();
    builder.Services.AddSingleton<IAiResolutionService, MockAiResolutionService>();
}

// Azure AI Search (with Cosmos DB fallback)
builder.Services.AddSingleton<IKnowledgeSearchService, AzureAiSearchService>();

var app = builder.Build();

// Initialize database and seed
await CosmosDbService.InitializeDatabaseAsync(cosmosClient, cosmosDbName);
await SeedData.SeedAsync(cosmosClient, cosmosDbName);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("DevCors");
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
