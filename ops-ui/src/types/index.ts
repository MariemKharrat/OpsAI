export interface Ticket {
  id: string;
  ticketNumber: string;
  subject: string;
  description: string;
  status: TicketStatus;
  priority: TicketPriority;
  category: TicketCategory;
  requesterName: string;
  requesterEmail: string;
  requesterDepartment: string;
  assignee?: string;
  tags: string[];
  createdAt: string;
  updatedAt: string;
  resolvedAt?: string;
}

export type TicketStatus = 'New' | 'Open' | 'InProgress' | 'Pending' | 'Resolved' | 'Closed';
export type TicketPriority = 'Low' | 'Medium' | 'High' | 'Critical';
export type TicketCategory = 'Hardware' | 'Software' | 'Network' | 'Security' | 'AccessPermissions' | 'Email' | 'Printing' | 'VPN' | 'AccountManagement' | 'Other';
export type SentimentType = 'Positive' | 'Neutral' | 'Frustrated' | 'Urgent';

export interface TicketListResponse {
  items: Ticket[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface KnowledgeArticle {
  id: string;
  title: string;
  content: string;
  category: TicketCategory;
  tags: string[];
  viewCount: number;
  helpfulCount: number;
  createdAt: string;
}

export interface TriageResult {
  id: string;
  ticketId: string;
  suggestedPriority: TicketPriority;
  suggestedCategory: TicketCategory;
  sentiment: SentimentType;
  confidence: number;
  reasoning: string;
  suggestedAssignee?: string;
  matchedArticleIds: string[];
  keyPhrases: string[];
  createdAt: string;
}

export interface Resolution {
  id: string;
  ticketId: string;
  summary: string;
  steps: string[];
  draftResponse: string;
  resolvedBy: string;
  resolutionType: string;
  timeToResolveMinutes?: number;
  satisfactionPrediction?: number;
  createdAt: string;
}

export interface AuditEntry {
  id: string;
  ticketId: string;
  action: string;
  actor: string;
  details: string;
  previousValue?: string;
  newValue?: string;
  timestamp: string;
}

export interface SimilarResolvedTicket {
  id: string;
  ticketNumber: string;
  subject: string;
  category: TicketCategory;
  resolvedAt?: string;
  resolutionSummary: string;
  resolutionSteps: string[];
}

export interface DashboardStats {
  totalTickets: number;
  openTickets: number;
  resolvedToday: number;
  criticalTickets: number;
  avgResolutionTimeMinutes: number;
  byStatus: Record<string, number>;
  byPriority: Record<string, number>;
  byCategory: Record<string, number>;
  recentTickets: Ticket[];
}

export interface ResolutionSuggestion {
  ticketId: string;
  steps: string[];
  relatedArticles: KnowledgeArticle[];
  generatedAt: string;
}

export interface DraftResponseResult {
  ticketId: string;
  draftResponse: string;
  resolutionSteps: string[];
  generatedAt: string;
}
