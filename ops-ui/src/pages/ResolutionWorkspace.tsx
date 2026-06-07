import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Bot, Search, Lightbulb, MessageSquare, AlertTriangle,
  CheckCircle2, Loader2, BookOpen, ArrowUpRight, Clock, History
} from 'lucide-react';
import { getTicket, getSimilarResolved, suggestResolution, draftResponse, escalateTicket, searchArticles } from '../services/api';
import { Card } from '../components/Card';
import ExpandableArticle from '../components/ExpandableArticle';
import { PriorityBadge, StatusBadge } from '../components/Badge';
import type { Ticket, KnowledgeArticle, ResolutionSuggestion, DraftResponseResult, SimilarResolvedTicket } from '../types';

export default function ResolutionWorkspace() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [ticket, setTicket] = useState<Ticket | null>(null);
  const [loading, setLoading] = useState(true);
  const [suggestion, setSuggestion] = useState<ResolutionSuggestion | null>(null);
  const [draft, setDraft] = useState<DraftResponseResult | null>(null);
  const [kbResults, setKbResults] = useState<KnowledgeArticle[]>([]);
  const [kbQuery, setKbQuery] = useState('');
  const [aiLoading, setAiLoading] = useState<string | null>(null);
  const [notes, setNotes] = useState('');
  const [draftText, setDraftText] = useState('');
  const [escalateReason, setEscalateReason] = useState('');
  const [showEscalate, setShowEscalate] = useState(false);
  const [similarTickets, setSimilarTickets] = useState<SimilarResolvedTicket[]>([]);
  const [similarLoading, setSimilarLoading] = useState(false);

  useEffect(() => {
    if (!id) return;
    getTicket(id).then(r => { setTicket(r.data); setLoading(false); }).catch(() => setLoading(false));
    setSimilarLoading(true);
    getSimilarResolved(id).then(r => setSimilarTickets(r.data)).catch(() => {}).finally(() => setSimilarLoading(false));
  }, [id]);

  const handleSuggestResolution = async () => {
    if (!id) return;
    setAiLoading('resolution');
    try {
      const res = await suggestResolution(id);
      setSuggestion(res.data);
    } finally { setAiLoading(null); }
  };

  const handleDraftResponse = async () => {
    if (!id) return;
    setAiLoading('draft');
    try {
      const res = await draftResponse(id);
      setDraft(res.data);
      setDraftText(res.data.draftResponse);
    } finally { setAiLoading(null); }
  };

  const handleKbSearch = async () => {
    if (!kbQuery.trim()) return;
    setAiLoading('kb');
    try {
      const res = await searchArticles(kbQuery);
      setKbResults(res.data);
    } finally { setAiLoading(null); }
  };

  const handleEscalate = async () => {
    if (!id || !escalateReason) return;
    setAiLoading('escalate');
    try {
      await escalateTicket(id, { reason: escalateReason });
      navigate(`/tickets/${id}/summary`);
    } finally { setAiLoading(null); }
  };

  const handleResolve = () => navigate(`/tickets/${id}/summary`);

  if (loading) return <div className="animate-pulse"><div className="h-96 bg-gray-200 rounded-xl" /></div>;
  if (!ticket) return <p className="text-red-500">Ticket not found.</p>;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm text-gray-500">{ticket.ticketNumber}</p>
          <h1 className="text-xl font-bold text-gray-900">{ticket.subject}</h1>
          <div className="flex items-center gap-3 mt-2">
            <StatusBadge status={ticket.status} />
            <PriorityBadge priority={ticket.priority} />
            <span className="text-sm text-gray-500">
              <Clock className="w-3.5 h-3.5 inline mr-1" />
              {new Date(ticket.createdAt).toLocaleDateString()}
            </span>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <button onClick={() => setShowEscalate(!showEscalate)}
            className="inline-flex items-center gap-1.5 px-3 py-2 border border-orange-300 text-orange-700 rounded-lg text-sm font-medium hover:bg-orange-50">
            <AlertTriangle className="w-4 h-4" /> Escalate
          </button>
          <button onClick={handleResolve}
            className="inline-flex items-center gap-1.5 px-4 py-2 bg-green-600 text-white rounded-lg text-sm font-medium hover:bg-green-700">
            <CheckCircle2 className="w-4 h-4" /> Resolve
          </button>
        </div>
      </div>

      {/* Escalation Panel */}
      {showEscalate && (
        <Card className="border-orange-200 bg-orange-50">
          <div className="space-y-3">
            <p className="text-sm font-medium text-orange-800">Escalate this ticket</p>
            <textarea rows={2} value={escalateReason} onChange={e => setEscalateReason(e.target.value)}
              className="w-full rounded-lg border border-orange-200 px-3 py-2 text-sm"
              placeholder="Reason for escalation..." />
            <button onClick={handleEscalate} disabled={!escalateReason || aiLoading === 'escalate'}
              className="px-4 py-2 bg-orange-600 text-white rounded-lg text-sm font-medium hover:bg-orange-700 disabled:opacity-50">
              {aiLoading === 'escalate' ? 'Escalating...' : 'Confirm Escalation'}
            </button>
          </div>
        </Card>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Left: Ticket Detail */}
        <div className="lg:col-span-2 space-y-6">
          <Card title="Ticket Description">
            <div className="space-y-4">
              <div className="flex items-center gap-4 text-sm">
                <span className="text-gray-500">Requester:</span>
                <span className="font-medium">{ticket.requesterName}</span>
                <span className="text-gray-400">•</span>
                <span className="text-gray-500">{ticket.requesterDepartment}</span>
              </div>
              <p className="text-sm text-gray-700 whitespace-pre-wrap bg-gray-50 p-4 rounded-lg">
                {ticket.description}
              </p>
              {ticket.tags.length > 0 && (
                <div className="flex flex-wrap gap-1.5">
                  {ticket.tags.map(tag => (
                    <span key={tag} className="px-2 py-0.5 bg-gray-100 text-gray-600 rounded text-xs">{tag}</span>
                  ))}
                </div>
              )}
            </div>
          </Card>

          {/* AI Suggested Resolution */}
          {suggestion && (
            <Card title="AI Resolution Steps" subtitle="Suggested troubleshooting approach">
              <ol className="space-y-2">
                {suggestion.steps.map((step, i) => (
                  <li key={i} className="flex items-start gap-3 text-sm">
                    <span className="flex-shrink-0 w-6 h-6 rounded-full bg-blue-100 text-blue-700 flex items-center justify-center text-xs font-medium">{i + 1}</span>
                    <span className="text-gray-700 pt-0.5">{step}</span>
                  </li>
                ))}
              </ol>
              {suggestion.relatedArticles.length > 0 && (
                <div className="mt-4 pt-4 border-t border-gray-100">
                  <p className="text-xs font-medium text-gray-500 mb-2">Related Articles</p>
                  {suggestion.relatedArticles.map(a => (
                    <p key={a.id} className="text-sm text-blue-600 flex items-center gap-1.5">
                      <BookOpen className="w-3.5 h-3.5" /> {a.title}
                    </p>
                  ))}
                </div>
              )}
            </Card>
          )}

          {/* Draft Response */}
          {draft && (
            <Card title="Draft Response" subtitle="Edit and send to requester">
              <textarea
                rows={8}
                value={draftText}
                onChange={e => setDraftText(e.target.value)}
                className="w-full rounded-lg border border-gray-300 px-4 py-3 text-sm text-gray-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 resize-y"
              />
              <div className="flex justify-end mt-3">
                <button
                  className="inline-flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg text-sm font-medium hover:bg-blue-700 transition-colors"
                >
                  <MessageSquare className="w-4 h-4" />
                  Send Reply
                </button>
              </div>
            </Card>
          )}

          {/* Internal Notes */}
          <Card title="Internal Notes">
            <textarea rows={4} value={notes} onChange={e => setNotes(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm"
              placeholder="Add internal notes about this ticket..." />
          </Card>
        </div>

        {/* Right: AI Tools Panel */}
        <div className="space-y-4">
          <Card title="AI Assistant" subtitle="Copilot tools">
            <div className="space-y-3">
              <button onClick={handleSuggestResolution} disabled={aiLoading === 'resolution'}
                className="w-full flex items-center gap-3 p-3 rounded-lg border border-gray-200 hover:bg-blue-50 hover:border-blue-200 transition-colors text-left">
                {aiLoading === 'resolution' ? <Loader2 className="w-5 h-5 text-blue-500 animate-spin" /> : <Lightbulb className="w-5 h-5 text-blue-500" />}
                <div>
                  <p className="text-sm font-medium text-gray-900">Suggest Resolution</p>
                  <p className="text-xs text-gray-500">Get AI troubleshooting steps</p>
                </div>
              </button>

              <button onClick={handleDraftResponse} disabled={aiLoading === 'draft'}
                className="w-full flex items-center gap-3 p-3 rounded-lg border border-gray-200 hover:bg-green-50 hover:border-green-200 transition-colors text-left">
                {aiLoading === 'draft' ? <Loader2 className="w-5 h-5 text-green-500 animate-spin" /> : <MessageSquare className="w-5 h-5 text-green-500" />}
                <div>
                  <p className="text-sm font-medium text-gray-900">Draft Response</p>
                  <p className="text-xs text-gray-500">Generate reply to requester</p>
                </div>
              </button>
            </div>
          </Card>

          {/* KB Search */}
          <Card title="Knowledge Base" subtitle="Search for solutions">
            <div className="space-y-3">
              <div className="flex gap-2">
                <input type="text" value={kbQuery} onChange={e => setKbQuery(e.target.value)}
                  onKeyDown={e => e.key === 'Enter' && handleKbSearch()}
                  className="flex-1 rounded-lg border border-gray-300 px-3 py-2 text-sm"
                  placeholder="Search KB..." />
                <button onClick={handleKbSearch} disabled={aiLoading === 'kb'}
                  className="p-2 bg-gray-100 rounded-lg hover:bg-gray-200">
                  {aiLoading === 'kb' ? <Loader2 className="w-4 h-4 animate-spin" /> : <Search className="w-4 h-4" />}
                </button>
              </div>
              {kbResults.length > 0 && (
                <div className="space-y-2 max-h-96 overflow-y-auto">
                  {kbResults.map(article => (
                    <ExpandableArticle key={article.id} article={article} compact />
                  ))}
                </div>
              )}
            </div>
          </Card>

          {/* Similar Resolved Tickets */}
          <Card title="Similar Resolved Tickets" subtitle="Same category, successfully resolved">
            {similarLoading ? (
              <div className="space-y-2 animate-pulse">
                {[1, 2].map(i => <div key={i} className="h-16 bg-gray-100 rounded" />)}
              </div>
            ) : similarTickets.length === 0 ? (
              <p className="text-xs text-gray-500 text-center py-4">No similar resolved tickets found.</p>
            ) : (
              <div className="space-y-3">
                {similarTickets.map(st => (
                  <div key={st.id} className="p-3 bg-gray-50 rounded-lg border border-gray-100">
                    <div className="flex items-center gap-2 mb-1">
                      <History className="w-3.5 h-3.5 text-green-600" />
                      <span className="text-xs font-mono text-gray-500">{st.ticketNumber}</span>
                      {st.resolvedAt && (
                        <span className="text-xs text-gray-400 ml-auto">
                          {new Date(st.resolvedAt).toLocaleDateString()}
                        </span>
                      )}
                    </div>
                    <p className="text-xs font-medium text-gray-900 mb-1.5">{st.subject}</p>
                    <div className="bg-green-50 border border-green-100 rounded p-2">
                      <p className="text-xs font-medium text-green-800 mb-1">Resolution:</p>
                      <p className="text-xs text-green-700">{st.resolutionSummary}</p>
                      {st.resolutionSteps.length > 0 && (
                        <ol className="mt-1.5 space-y-0.5 list-decimal list-inside">
                          {st.resolutionSteps.map((step, i) => (
                            <li key={i} className="text-xs text-green-700">{step}</li>
                          ))}
                        </ol>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </Card>

          {/* Ticket Meta */}
          <Card title="Details">
            <div className="space-y-2 text-sm">
              <div className="flex justify-between">
                <span className="text-gray-500">Assignee</span>
                <span className="font-medium">{ticket.assignee || 'Unassigned'}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-500">Category</span>
                <span className="font-medium">{ticket.category}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-500">Created</span>
                <span className="font-medium">{new Date(ticket.createdAt).toLocaleString()}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-gray-500">Email</span>
                <span className="font-medium text-xs">{ticket.requesterEmail}</span>
              </div>
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}
