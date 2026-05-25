# OpsAI — AI-Powered IT Support Copilot

An enterprise IT support ticketing system with AI-powered triage, knowledge retrieval, and resolution assistance. Built with .NET 10, React TypeScript, and Azure AI services.

## Architecture

| Layer | Technology |
|-------|-----------|
| Frontend | React 19 + TypeScript + Vite + Tailwind CSS |
| Backend | .NET 10 Web API |
| Database | Azure Cosmos DB (NoSQL) |
| AI | Azure OpenAI (GPT-4o) + Azure AI Search + Azure AI Foundry |

---

## Application Workflow

The complete end-to-end workflow from ticket submission to resolution, showing how each feature maps to Azure AI services:

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                           OpsAI APPLICATION WORKFLOW                             │
└─────────────────────────────────────────────────────────────────────────────────┘

 ┌──────────┐        ┌──────────────┐        ┌──────────────┐        ┌──────────┐
 │  TICKET  │──────▶ │  AI TRIAGE   │──────▶ │  RESOLUTION  │──────▶ │ RESOLVED │
 │  INTAKE  │        │   RESULT     │        │  WORKSPACE   │        │ SUMMARY  │
 └──────────┘        └──────────────┘        └──────────────┘        └──────────┘
      │                     │                       │                       │
      ▼                     ▼                       ▼                       ▼
 ┌──────────┐        ┌──────────────┐        ┌──────────────┐        ┌──────────┐
 │  Cosmos  │        │ Azure OpenAI │        │ Azure AI     │        │  Cosmos  │
 │    DB    │        │ + AI Search  │        │ Search +     │        │    DB    │
 │          │        │ + AI Foundry │        │ OpenAI       │        │          │
 └──────────┘        └──────────────┘        └──────────────┘        └──────────┘
```

### Step 1: Dashboard (Operations Overview)

```
┌─────────────────────────────────────────────────────┐
│                    DASHBOARD                         │
├─────────────────────────────────────────────────────┤
│  Ticket Stats │ Priority Chart │ Recent Tickets     │
│  Open: 18     │ Critical: 4   │ [table of tickets] │
│  Resolved: 2  │ High: 5       │                    │
│  Total: 23    │ Medium: 8     │                    │
└─────────────────────────────────────────────────────┘
         │
         │  Data Source: Cosmos DB → GET /api/tickets/stats
         ▼
    ┌────────────┐
    │ Cosmos DB  │  Queries all tickets, computes aggregations
    │  (NoSQL)   │  by status, priority, category in real-time
    └────────────┘
```

**Azure Service**: Azure Cosmos DB NoSQL  
**What it does**: Stores all tickets, articles, audit entries. Provides real-time aggregation queries for dashboard metrics.

---

### Step 2: Ticket Intake (Submission)

```
┌─────────────────────────────────────────────────────┐
│                 TICKET INTAKE                        │
├─────────────────────────────────────────────────────┤
│  Requester: [name] [email] [department]             │
│  Subject:   [brief issue summary]                   │
│  Description: [detailed problem description]        │
│  Category:  [optional - or let AI classify]         │
│  Priority:  [optional - or let AI assess]           │
│                                                     │
│  ┌─────────────────────────────────────────────┐    │
│  │ 🤖 AI-Powered Triage  [ON/OFF toggle]      │    │
│  │ Auto-classify priority, category, find KB   │    │
│  └─────────────────────────────────────────────┘    │
│                                                     │
│                          [Submit Ticket] ──────────────┐
└─────────────────────────────────────────────────────┘  │
                                                         │
         ┌───────────────────────────────────────────────┘
         ▼
    ┌────────────┐     ┌──────────────────┐
    │ Cosmos DB  │────▶│ AI Triage Engine  │  (if toggle ON)
    │ Save Ticket│     │ POST /ai/triage   │
    └────────────┘     └──────────────────┘
```

**Azure Service**: Azure Cosmos DB NoSQL  
**What it does**: Persists the new ticket. If AI triage is enabled, immediately triggers the classification pipeline.

---

### Step 3: AI Triage Result (Classification)

```
┌─────────────────────────────────────────────────────────────────────┐
│                      AI TRIAGE RESULT                                │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  🧠 AI Confidence Score: ████████████████████░░ 92%                 │
│                                                                     │
│  ┌────────────┐ ┌────────────┐ ┌────────────┐ ┌────────────┐       │
│  │  Priority  │ │  Category  │ │ Sentiment  │ │  Assignee  │       │
│  │    HIGH    │ │    VPN     │ │   Urgent   │ │Network Team│       │
│  └────────────┘ └────────────┘ └────────────┘ └────────────┘       │
│                                                                     │
│  Key Phrases: [vpn] [remote-work] [connection timeout]              │
│                                                                     │
│  Reasoning: "Based on analysis, this is a VPN connectivity          │
│  issue with high priority due to deadline pressure..."              │
│                                                                     │
│  📚 Matched Knowledge Articles:                                     │
│  ├─ VPN Connection Troubleshooting Guide (445 helpful)              │
│  ├─ WiFi Connectivity Issues (267 helpful)                          │
│  └─ Network Setup Guide (198 helpful)                               │
│                                                                     │
│                              [Continue to Workspace →]              │
└─────────────────────────────────────────────────────────────────────┘
         │
         │ Powered by:
         ▼
┌─────────────────────────────────────────────────────────────────────┐
│                                                                     │
│  ┌─────────────────┐    ┌─────────────────┐    ┌────────────────┐  │
│  │ Azure OpenAI    │    │ Azure AI Search │    │ Azure AI       │  │
│  │ (GPT-4o)        │    │                 │    │ Foundry        │  │
│  ├─────────────────┤    ├─────────────────┤    ├────────────────┤  │
│  │ • Classify      │    │ • Semantic KB   │    │ • Orchestrate  │  │
│  │   priority      │    │   search        │    │   AI pipeline  │  │
│  │ • Detect        │    │ • Vector        │    │ • Manage       │  │
│  │   category      │    │   similarity    │    │   prompts      │  │
│  │ • Analyze       │    │   matching      │    │ • Monitor      │  │
│  │   sentiment     │    │ • Rank article  │    │   confidence   │  │
│  │ • Extract key   │    │   relevance     │    │ • A/B test     │  │
│  │   phrases       │    │                 │    │   models       │  │
│  │ • Suggest       │    │                 │    │                │  │
│  │   assignee      │    │                 │    │                │  │
│  └─────────────────┘    └─────────────────┘    └────────────────┘  │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

**Azure Services Used**:

| Service | Role | API Call |
|---------|------|----------|
| **Azure OpenAI (GPT-4o)** | Classifies priority, category, sentiment. Extracts key phrases. Generates reasoning explanation. Suggests assignee based on expertise mapping. | `POST /ai/triage/{ticketId}` |
| **Azure AI Search** | Performs semantic/vector search over KB articles. Matches ticket description against indexed knowledge base. Returns ranked relevant articles. | Internal call during triage |
| **Azure AI Foundry** | Orchestrates the multi-step triage pipeline. Manages prompt templates, model deployments, and confidence scoring. Provides observability. | Pipeline orchestration |

---

### Step 4: Resolution Workspace (AI-Assisted Troubleshooting)

```
┌────────────────────────────────────────────────────────────────────────────────┐
│                        RESOLUTION WORKSPACE                                    │
├──────────────────────────────────────────────┬─────────────────────────────────┤
│                                              │                                 │
│  📋 Ticket: INC-2024-001                     │  🤖 AI ASSISTANT                │
│  Status: Open  Priority: High                │                                 │
│                                              │  ┌───────────────────────────┐  │
│  ┌──────────────────────────────────────┐    │  │ 💡 Suggest Resolution     │  │
│  │ Description:                         │    │  │ Get AI troubleshooting    │  │
│  │ "Cannot connect to VPN from home..." │    │  │ steps                     │  │
│  └──────────────────────────────────────┘    │  └───────────────────────────┘  │
│                                              │                                 │
│  ┌──────────────────────────────────────┐    │  ┌───────────────────────────┐  │
│  │ AI Resolution Steps:                 │    │  │ 💬 Draft Response         │  │
│  │ 1. Verify internet connectivity      │    │  │ Generate reply to         │  │
│  │ 2. Check VPN client version          │    │  │ requester                 │  │
│  │ 3. Try alternate VPN server          │    │  └───────────────────────────┘  │
│  │ 4. Flush DNS cache                   │    │                                 │
│  │ 5. Check firewall rules              │    │  ┌───────────────────────────┐  │
│  │ 6. Reference KB: "VPN Troubleshoot"  │    │  │ 🔍 Knowledge Base         │  │
│  └──────────────────────────────────────┘    │  │ [search box]              │  │
│                                              │  │ • VPN Troubleshooting     │  │
│  ┌──────────────────────────────────────┐    │  │ • WiFi Guide              │  │
│  │ Draft Response:                      │    │  └───────────────────────────┘  │
│  │ "Hi Sarah, Thank you for reaching    │    │                                 │
│  │  out regarding your VPN issue..."    │    │  ┌───────────────────────────┐  │
│  └──────────────────────────────────────┘    │  │ Ticket Details            │  │
│                                              │  │ Assignee: Network Team    │  │
│  ┌──────────────────────────────────────┐    │  │ Category: VPN             │  │
│  │ 📝 Internal Notes                    │    │  │ Created: May 11, 2026     │  │
│  │ [text area for agent notes]          │    │  └───────────────────────────┘  │
│  └──────────────────────────────────────┘    │                                 │
│                                              │                                 │
│  [⚠️ Escalate]              [✅ Resolve]     │                                 │
├──────────────────────────────────────────────┴─────────────────────────────────┤
│                                                                                │
│  AI Feature → Azure Service Mapping:                                           │
│                                                                                │
│  💡 Suggest Resolution ──▶ Azure OpenAI (GPT-4o)                               │
│     • Analyzes ticket context + matched KB articles                            │
│     • Generates step-by-step troubleshooting guide                             │
│     • Tailors steps to specific issue details                                  │
│                                                                                │
│  💬 Draft Response ──▶ Azure OpenAI (GPT-4o)                                   │
│     • Takes ticket + resolution steps as context                               │
│     • Generates professional, empathetic response                              │
│     • Includes estimated resolution time                                       │
│                                                                                │
│  🔍 Knowledge Base Search ──▶ Azure AI Search                                  │
│     • Semantic search over indexed KB articles                                 │
│     • Vector embeddings for meaning-based matching                             │
│     • Ranked results by relevance + helpfulness                                │
│                                                                                │
│  ⚠️ Escalate ──▶ Azure AI Foundry (workflow orchestration)                     │
│     • Updates priority, assignee, and status                                   │
│     • Triggers notification pipeline                                           │
│     • Logs escalation in audit trail                                           │
│                                                                                │
└────────────────────────────────────────────────────────────────────────────────┘
```

**Azure Services Used**:

| Feature | Azure Service | What Happens |
|---------|--------------|--------------|
| **Suggest Resolution** | Azure OpenAI (GPT-4o) | Sends ticket description + related KB content as context. GPT-4o generates ordered troubleshooting steps specific to the issue. |
| **Draft Response** | Azure OpenAI (GPT-4o) | Composes a professional customer-facing reply incorporating resolution steps, estimated timeline, and empathetic tone. |
| **KB Search** | Azure AI Search | Semantic search with vector embeddings. User types query → Azure AI Search returns ranked articles by meaning similarity, not just keywords. |
| **Escalation** | Azure AI Foundry | Orchestrates the escalation workflow: priority bump, reassignment, notification triggers, audit logging. |

---

### Step 5: Resolution Summary (Closure & Audit)

```
┌─────────────────────────────────────────────────────────────────────┐
│                     RESOLUTION SUMMARY                               │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌───────────┐ ┌───────────┐ ┌───────────┐ ┌───────────┐          │
│  │  Time to  │ │  Status   │ │ Priority  │ │Satisfaction│          │
│  │  Resolve  │ │           │ │           │ │ Prediction │          │
│  │   2h 34m  │ │ Resolved ✓│ │   High    │ │    87%     │          │
│  └───────────┘ └───────────┘ └───────────┘ └───────────┘          │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │ Close Ticket                                                 │   │
│  │ Resolution: [describe how issue was resolved]                │   │
│  │                                    [✅ Mark as Resolved]      │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  📜 AUDIT TRAIL                                                     │
│  ─────────────────────────────────────────────────────────         │
│  ● Created ─ Sarah Chen ─ "Ticket created via email"               │
│  │  May 11, 2026 2:00 AM                                          │
│  │                                                                  │
│  ● Assigned ─ System ─ "Auto-assigned to Mike Torres"              │
│  │  May 11, 2026 2:05 AM                                          │
│  │                                                                  │
│  ● AI Triage ─ AI System ─ "Classified as VPN, High priority"      │
│  │  May 11, 2026 2:05 AM                                          │
│  │                                                                  │
│  ● Status Changed ─ Mike Torres ─ "Began investigation"            │
│  │  May 11, 2026 3:00 AM                                          │
│  │                                                                  │
│  ● Resolved ─ Mike Torres ─ "VPN config updated, user verified"    │
│     May 11, 2026 4:34 AM                                          │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
         │
         │ Powered by:
         ▼
┌─────────────────────────────────────────────────────────────────────┐
│  ┌─────────────────┐    ┌─────────────────┐                        │
│  │ Azure OpenAI    │    │ Cosmos DB       │                        │
│  │ (GPT-4o)        │    │                 │                        │
│  ├─────────────────┤    ├─────────────────┤                        │
│  │ • Satisfaction  │    │ • Persist       │                        │
│  │   prediction    │    │   resolution    │                        │
│  │ • Resolution    │    │ • Store audit   │                        │
│  │   quality score │    │   trail         │                        │
│  │                 │    │ • Update ticket │                        │
│  │                 │    │   status        │                        │
│  └─────────────────┘    └─────────────────┘                        │
└─────────────────────────────────────────────────────────────────────┘
```

**Azure Services Used**:

| Feature | Azure Service | What Happens |
|---------|--------------|--------------|
| **Satisfaction Prediction** | Azure OpenAI (GPT-4o) | Analyzes resolution quality, response time, and ticket complexity to predict customer satisfaction score (0-100%). |
| **Audit Trail Storage** | Azure Cosmos DB | Every action (create, assign, triage, escalate, resolve) is stored as an immutable audit entry with actor, timestamp, and details. |
| **Resolution Persistence** | Azure Cosmos DB | Stores resolution summary, steps taken, time-to-resolve, and links back to the ticket. |

---

## Complete Azure AI Service Mapping

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    AZURE AI SERVICES IN OpsAI                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │                    AZURE AI FOUNDRY (Orchestration Layer)             │   │
│  │                                                                      │   │
│  │  • Project management for all AI models and deployments              │   │
│  │  • Prompt template versioning and management                         │   │
│  │  • Model evaluation and A/B testing                                  │   │
│  │  • Observability: latency, token usage, confidence tracking          │   │
│  │  • Responsible AI: content filtering, bias detection                 │   │
│  └──────────────────────────┬───────────────────────────────────────────┘   │
│                             │                                               │
│              ┌──────────────┼──────────────┐                                │
│              ▼              ▼              ▼                                 │
│  ┌────────────────┐ ┌─────────────┐ ┌──────────────────┐                   │
│  │ AZURE OPENAI   │ │ AZURE AI    │ │ AZURE COSMOS DB  │                   │
│  │ (GPT-4o)       │ │ SEARCH      │ │ (NoSQL)          │                   │
│  ├────────────────┤ ├─────────────┤ ├──────────────────┤                   │
│  │                │ │             │ │                  │                   │
│  │ TRIAGE:        │ │ KNOWLEDGE:  │ │ DATA STORE:      │                   │
│  │ • Priority     │ │ • Semantic  │ │ • Tickets        │                   │
│  │   classification│ │   search   │ │ • KB Articles    │                   │
│  │ • Category     │ │ • Vector   │ │ • Triage Results │                   │
│  │   detection    │ │   index    │ │ • Resolutions    │                   │
│  │ • Sentiment    │ │ • Hybrid   │ │ • Audit Entries  │                   │
│  │   analysis     │ │   ranking  │ │                  │                   │
│  │ • Key phrase   │ │ • Faceted  │ │ QUERIES:         │                   │
│  │   extraction   │ │   filters  │ │ • Aggregations   │                   │
│  │                │ │            │ │ • Filtering      │                   │
│  │ RESOLUTION:    │ │ INGESTION: │ │ • Pagination     │                   │
│  │ • Troubleshoot │ │ • Index KB │ │ • Cross-partition│                   │
│  │   steps        │ │   articles │ │                  │                   │
│  │ • Response     │ │ • Vectorize│ │                  │                   │
│  │   drafting     │ │   content  │ │                  │                   │
│  │ • Satisfaction │ │ • Update   │ │                  │                   │
│  │   prediction   │ │   on change│ │                  │                   │
│  │                │ │            │ │                  │                   │
│  └────────────────┘ └─────────────┘ └──────────────────┘                   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Service-to-Feature Matrix

| Feature | Azure OpenAI | Azure AI Search | Azure AI Foundry | Cosmos DB |
|---------|:---:|:---:|:---:|:---:|
| Dashboard Stats | | | | ✅ |
| Ticket CRUD | | | | ✅ |
| Priority Classification | ✅ | | ✅ | |
| Category Detection | ✅ | | ✅ | |
| Sentiment Analysis | ✅ | | ✅ | |
| Key Phrase Extraction | ✅ | | | |
| Assignee Suggestion | ✅ | | | |
| KB Article Matching | | ✅ | ✅ | |
| KB Semantic Search | | ✅ | | |
| Resolution Steps | ✅ | ✅ | ✅ | |
| Response Drafting | ✅ | | ✅ | |
| Satisfaction Prediction | ✅ | | | |
| Escalation Workflow | | | ✅ | ✅ |
| Audit Trail | | | | ✅ |

---

## Data Flow Diagram

```
User (Browser)
     │
     ▼
┌─────────────┐     ┌──────────────────────────────────────────────┐
│  React SPA  │────▶│              .NET 10 Web API                 │
│  (Vite)     │◀────│                                              │
│             │     │  ┌────────────┐  ┌─────────────────────────┐ │
│  Pages:     │     │  │ Controllers│  │      Services           │ │
│  • Dashboard│     │  │            │  │                         │ │
│  • Intake   │     │  │ • Tickets  │  │ • IAiTriageService      │ │
│  • Triage   │     │  │ • AI       │──▶│ • IAiResolutionService  │ │
│  • Workspace│     │  │ • Knowledge│  │ • IKnowledgeSearchSvc   │ │
│  • Summary  │     │  │ • Audit    │  │                         │ │
│             │     │  └────────────┘  └───────────┬─────────────┘ │
└─────────────┘     │                              │               │
                    │                              ▼               │
                    │  ┌───────────────────────────────────────┐   │
                    │  │           Azure Services              │   │
                    │  │                                       │   │
                    │  │  ┌─────────┐ ┌────────┐ ┌─────────┐  │   │
                    │  │  │ OpenAI  │ │Search  │ │Cosmos DB│  │   │
                    │  │  │ GPT-4o  │ │        │ │         │  │   │
                    │  │  └─────────┘ └────────┘ └─────────┘  │   │
                    │  └───────────────────────────────────────┘   │
                    └──────────────────────────────────────────────┘
```

---

## Getting Started

### Prerequisites
- .NET 10 SDK
- Node.js 20+
- Azure Cosmos DB Emulator (or Azure Cosmos DB account)

### Backend

```bash
cd OpsAI.Api
# Update CosmosDb connection string in appsettings.json if needed
dotnet run
```

API runs at `http://localhost:5150`

### Frontend

```bash
cd ops-ui
npm install
npm run dev
```

UI runs at `http://localhost:5173` (proxies API calls to backend)

---

## API Endpoints

### Tickets
- `GET /api/tickets` — List tickets (with filtering/pagination)
- `GET /api/tickets/{id}` — Get ticket detail
- `POST /api/tickets` — Create ticket
- `PUT /api/tickets/{id}` — Update ticket
- `DELETE /api/tickets/{id}` — Delete ticket
- `GET /api/tickets/stats` — Dashboard statistics

### AI
- `POST /api/ai/triage/{ticketId}` — Run AI triage on a ticket
- `GET /api/ai/triage/{ticketId}` — Get existing triage result
- `POST /api/ai/suggest-resolution/{ticketId}` — Get AI troubleshooting steps
- `POST /api/ai/draft-response/{ticketId}` — Generate customer response
- `POST /api/ai/escalate/{ticketId}` — Escalate ticket to senior support
- `POST /api/ai/resolve/{ticketId}` — Mark ticket resolved with summary

### Knowledge Base
- `GET /api/knowledge` — List all articles
- `GET /api/knowledge/{id}` — Get article by ID
- `GET /api/knowledge/search?q=term` — Semantic search over articles

### Audit
- `GET /api/audit/{ticketId}` — Get full audit trail for a ticket

---

## Seed Data

The app seeds on first run with:
- **23 realistic IT tickets** — VPN issues, password resets, phishing reports, hardware failures, access requests, software installs, network outages, security incidents
- **15 knowledge base articles** — Password reset guide, VPN troubleshooting, MFA setup, Outlook fixes, printer guide, WiFi issues, software requests, onboarding checklist, BitLocker recovery, shared drives, phishing response, docking stations, meeting rooms, battery optimization, Azure DevOps access
- **14 audit trail entries** — Ticket creation, AI triage events, status changes, escalations, resolutions

---

## Azure AI Integration

### Current State: ✅ LIVE with Azure AI Services

The application is **connected to real Azure AI services** via Azure AI Foundry. AI features are powered by GPT-4o with intelligent fallback to rule-based classification if the service is unavailable.

| Interface | Live Implementation | Fallback | Azure Service |
|-----------|-------------------|----------|---------------|
| `IAiTriageService` | `AzureOpenAiTriageService` | `MockAiTriageService` | Azure OpenAI (GPT-4o) |
| `IAiResolutionService` | `AzureOpenAiResolutionService` | `MockAiResolutionService` | Azure OpenAI (GPT-4o) |
| `IKnowledgeSearchService` | `AzureAiSearchService` | Cosmos DB text search | Azure AI Search |

### How It Works

```
┌─────────────────────────────────────────────────────────────────────┐
│                   LIVE AZURE AI PIPELINE                             │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  User submits ticket                                                │
│       │                                                             │
│       ▼                                                             │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │ AzureOpenAiTriageService                                    │    │
│  │                                                             │    │
│  │  1. Builds system prompt with classification instructions   │    │
│  │  2. Sends ticket subject + description to GPT-4o            │    │
│  │  3. GPT-4o returns structured JSON:                         │    │
│  │     • Priority (Low/Medium/High/Critical)                   │    │
│  │     • Category (VPN/Security/Network/etc.)                  │    │
│  │     • Sentiment (Positive/Neutral/Frustrated/Urgent)        │    │
│  │     • Confidence score (0.0 - 1.0)                          │    │
│  │     • Reasoning explanation                                 │    │
│  │     • Suggested assignee                                    │    │
│  │     • Key phrases extracted                                 │    │
│  │  4. Queries Cosmos DB for matching KB articles              │    │
│  │  5. Returns complete TriageResult                           │    │
│  │                                                             │    │
│  │  ⚠️ On failure: Falls back to MockAiTriageService           │    │
│  └─────────────────────────────────────────────────────────────┘    │
│       │                                                             │
│       ▼                                                             │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │ AzureOpenAiResolutionService                                │    │
│  │                                                             │    │
│  │  Suggest Resolution:                                        │    │
│  │  • Sends ticket + matched KB articles as context to GPT-4o  │    │
│  │  • GPT-4o generates 4-7 actionable troubleshooting steps    │    │
│  │  • Steps are grounded in KB content (RAG pattern)           │    │
│  │                                                             │    │
│  │  Draft Response:                                            │    │
│  │  • Takes ticket + resolution steps as input                 │    │
│  │  • GPT-4o composes professional customer-facing email       │    │
│  │  • Includes empathetic tone + estimated resolution time     │    │
│  │                                                             │    │
│  │  Satisfaction Prediction:                                   │    │
│  │  • Analyzes ticket priority, time open, resolution quality  │    │
│  │  • Returns predicted CSAT score (0.0 - 1.0)                 │    │
│  └─────────────────────────────────────────────────────────────┘    │
│       │                                                             │
│       ▼                                                             │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │ AzureAiSearchService                                        │    │
│  │                                                             │    │
│  │  • Attempts semantic search via Azure AI Search index       │    │
│  │  • If index not available, falls back to Cosmos DB          │    │
│  │    CONTAINS() text search over KB articles                  │    │
│  │  • Returns ranked articles by relevance/helpfulness         │    │
│  └─────────────────────────────────────────────────────────────┘    │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### Configuration (`appsettings.json`)

```json
{
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=https://your-account.documents.azure.com:443/;AccountKey=...",
    "DatabaseName": "OpsAI"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-foundry.services.ai.azure.com",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-4o"
  },
  "AzureAISearch": {
    "Endpoint": "https://your-search.search.windows.net",
    "ApiKey": "your-api-key",
    "IndexName": "opsai-knowledge"
  }
}
```

> **Note**: For AI Foundry endpoints, use the base service URL (e.g., `https://name.services.ai.azure.com`) — do NOT include `/api/projects/...` path. The SDK handles routing internally.

### Automatic Fallback Behavior

The application is designed with **graceful degradation**:

```
Azure OpenAI available?
  ├── YES → Use GPT-4o for triage, resolution, drafting
  └── NO  → Fall back to rule-based classification (MockAiTriageService)
             - Keyword matching for category/priority
             - Template-based resolution steps
             - Pre-built response templates

Azure AI Search available?
  ├── YES → Semantic search with vector embeddings
  └── NO  → Cosmos DB CONTAINS() text search over KB articles
```

This means the app **never breaks** — it always returns results, just with varying intelligence levels.

### Service Registration Logic (`Program.cs`)

```csharp
// If Azure OpenAI credentials are configured → use real AI
if (!string.IsNullOrEmpty(openAiEndpoint) && !string.IsNullOrEmpty(openAiKey))
{
    var azureOpenAiClient = new AzureOpenAIClient(endpoint, credential);
    var chatClient = azureOpenAiClient.GetChatClient("gpt-4o");

    builder.Services.AddSingleton<IAiTriageService, AzureOpenAiTriageService>();
    builder.Services.AddSingleton<IAiResolutionService, AzureOpenAiResolutionService>();
}
else
{
    // No credentials → use mock (rule-based) implementations
    builder.Services.AddSingleton<IAiTriageService, MockAiTriageService>();
    builder.Services.AddSingleton<IAiResolutionService, MockAiResolutionService>();
}

// AI Search always uses AzureAiSearchService (has internal Cosmos DB fallback)
builder.Services.AddSingleton<IKnowledgeSearchService, AzureAiSearchService>();
```

---

## Project Structure

```
OpsAI/
├── OpsAI.Api/                    # .NET 10 Backend
│   ├── Controllers/
│   │   ├── TicketsController.cs  # CRUD + stats
│   │   ├── AiController.cs       # Triage, resolution, escalation
│   │   ├── KnowledgeController.cs # KB search
│   │   └── AuditController.cs    # Audit trail
│   ├── Services/
│   │   ├── Interfaces.cs         # IAiTriageService, IAiResolutionService, IKnowledgeSearchService
│   │   ├── AzureOpenAiTriageService.cs      # ✅ LIVE - GPT-4o classification
│   │   ├── AzureOpenAiResolutionService.cs  # ✅ LIVE - GPT-4o resolution/drafting
│   │   ├── AzureAiSearchService.cs          # ✅ LIVE - Semantic KB search
│   │   ├── MockAiTriageService.cs           # Fallback - rule-based classification
│   │   ├── MockAiResolutionService.cs       # Fallback - template responses
│   │   └── MockKnowledgeSearchService.cs    # Fallback - Cosmos text search
│   ├── Models/
│   │   ├── Ticket.cs
│   │   ├── KnowledgeArticle.cs
│   │   ├── TriageResult.cs
│   │   ├── Resolution.cs
│   │   ├── AuditEntry.cs
│   │   ├── Enums.cs
│   │   └── Dto/Requests.cs
│   ├── Data/
│   │   ├── CosmosDbService.cs    # Repository layer
│   │   └── SeedData.cs           # 23 tickets + 15 articles + 14 audits
│   ├── Program.cs                # DI, Cosmos init, Azure AI setup, middleware
│   └── appsettings.json          # Config (Cosmos, OpenAI, AI Search)
│
├── ops-ui/                       # React Frontend
│   ├── src/
│   │   ├── pages/
│   │   │   ├── Dashboard.tsx
│   │   │   ├── TicketIntake.tsx
│   │   │   ├── TriageResultPage.tsx
│   │   │   ├── ResolutionWorkspace.tsx
│   │   │   └── ResolutionSummary.tsx
│   │   ├── components/
│   │   │   ├── Layout.tsx        # Sidebar + shell
│   │   │   ├── Badge.tsx         # Priority/Status/Category badges
│   │   │   └── Card.tsx          # Card + StatCard
│   │   ├── services/api.ts       # Axios API client
│   │   ├── types/index.ts        # TypeScript interfaces
│   │   ├── App.tsx               # Router
│   │   └── main.tsx              # Entry point
│   └── vite.config.ts            # Vite + Tailwind + API proxy
│
├── README.md
└── .gitignore
```
