import axios from 'axios';
import type {
  Ticket, TicketListResponse, DashboardStats,
  KnowledgeArticle, TriageResult, ResolutionSuggestion,
  DraftResponseResult, Resolution, AuditEntry
} from '../types';

const api = axios.create({ baseURL: '/api' });

// Tickets
export const getTickets = (params?: {
  status?: string; priority?: string; category?: string; page?: number; pageSize?: number;
}) => api.get<TicketListResponse>('/tickets', { params });

export const getTicket = (id: string) => api.get<Ticket>(`/tickets/${id}`);

export const createTicket = (data: {
  subject: string; description: string; requesterName: string;
  requesterEmail: string; requesterDepartment: string;
  category?: string; priority?: string; tags?: string[];
  triggerAiTriage?: boolean;
}) => api.post<Ticket>('/tickets', data);

export const updateTicket = (id: string, data: Partial<Ticket>) =>
  api.put<Ticket>(`/tickets/${id}`, data);

export const deleteTicket = (id: string) => api.delete(`/tickets/${id}`);

export const getStats = () => api.get<DashboardStats>('/tickets/stats');

// Knowledge
export const getArticles = (category?: string) =>
  api.get<KnowledgeArticle[]>('/knowledge', { params: { category } });

export const getArticle = (id: string) => api.get<KnowledgeArticle>(`/knowledge/${id}`);

export const searchArticles = (q: string) =>
  api.get<KnowledgeArticle[]>('/knowledge/search', { params: { q } });

// AI
export const triageTicket = (ticketId: string) =>
  api.post<TriageResult>(`/ai/triage/${ticketId}`);

export const getTriageResult = (ticketId: string) =>
  api.get<TriageResult>(`/ai/triage/${ticketId}`);

export const suggestResolution = (ticketId: string) =>
  api.post<ResolutionSuggestion>(`/ai/suggest-resolution/${ticketId}`);

export const draftResponse = (ticketId: string) =>
  api.post<DraftResponseResult>(`/ai/draft-response/${ticketId}`);

export const escalateTicket = (ticketId: string, data: {
  reason: string; escalateTo?: string; escalatedBy?: string;
}) => api.post(`/ai/escalate/${ticketId}`, data);

export const resolveTicket = (ticketId: string, data: {
  summary: string; steps?: string[]; responseSent?: string; resolvedBy?: string;
}) => api.post<Resolution>(`/ai/resolve/${ticketId}`, data);

// Audit
export const getAuditTrail = (ticketId: string) =>
  api.get<AuditEntry[]>(`/audit/${ticketId}`);
