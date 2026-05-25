import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  CheckCircle2, Clock, ArrowLeft, User, Activity,
  ThumbsUp, Loader2, AlertCircle
} from 'lucide-react';
import { getTicket, getAuditTrail, resolveTicket } from '../services/api';
import { Card } from '../components/Card';
import { PriorityBadge, StatusBadge } from '../components/Badge';
import type { Ticket, AuditEntry } from '../types';

export default function ResolutionSummary() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [ticket, setTicket] = useState<Ticket | null>(null);
  const [auditTrail, setAuditTrail] = useState<AuditEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const [resolving, setResolving] = useState(false);
  const [resolutionSummary, setResolutionSummary] = useState('');
  const [isResolved, setIsResolved] = useState(false);

  useEffect(() => {
    if (!id) return;
    Promise.all([
      getTicket(id),
      getAuditTrail(id),
    ]).then(([ticketRes, auditRes]) => {
      setTicket(ticketRes.data);
      setAuditTrail(auditRes.data);
      setIsResolved(ticketRes.data.status === 'Resolved' || ticketRes.data.status === 'Closed');
      setLoading(false);
    }).catch(() => setLoading(false));
  }, [id]);

  const handleResolve = async () => {
    if (!id || !resolutionSummary) return;
    setResolving(true);
    try {
      await resolveTicket(id, {
        summary: resolutionSummary,
        resolvedBy: 'IT Support Agent',
      });
      setIsResolved(true);
      // Refresh audit trail
      const auditRes = await getAuditTrail(id);
      setAuditTrail(auditRes.data);
      const ticketRes = await getTicket(id);
      setTicket(ticketRes.data);
    } finally { setResolving(false); }
  };

  if (loading) return <div className="animate-pulse"><div className="h-96 bg-gray-200 rounded-xl" /></div>;
  if (!ticket) return <p className="text-red-500">Ticket not found.</p>;

  const timeOpen = ticket.resolvedAt
    ? Math.round((new Date(ticket.resolvedAt).getTime() - new Date(ticket.createdAt).getTime()) / 60000)
    : Math.round((Date.now() - new Date(ticket.createdAt).getTime()) / 60000);

  const formatDuration = (mins: number) => {
    if (mins < 60) return `${mins}m`;
    if (mins < 1440) return `${Math.round(mins / 60)}h ${mins % 60}m`;
    return `${Math.round(mins / 1440)}d ${Math.round((mins % 1440) / 60)}h`;
  };

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <button onClick={() => navigate(`/tickets/${id}/workspace`)}
            className="inline-flex items-center gap-1 text-sm text-gray-500 hover:text-gray-700 mb-2">
            <ArrowLeft className="w-3.5 h-3.5" /> Back to Workspace
          </button>
          <h1 className="text-2xl font-bold text-gray-900">Resolution Summary</h1>
          <p className="text-gray-500 mt-1">{ticket.ticketNumber} — {ticket.subject}</p>
        </div>
        {isResolved && (
          <div className="flex items-center gap-2 px-4 py-2 bg-green-50 border border-green-200 rounded-lg">
            <CheckCircle2 className="w-5 h-5 text-green-600" />
            <span className="text-sm font-medium text-green-700">Resolved</span>
          </div>
        )}
      </div>

      {/* Summary Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div className="bg-white rounded-xl border p-4 text-center">
          <Clock className="w-5 h-5 text-gray-400 mx-auto mb-1" />
          <p className="text-xs text-gray-500">Time to Resolve</p>
          <p className="text-lg font-bold text-gray-900">{formatDuration(timeOpen)}</p>
        </div>
        <div className="bg-white rounded-xl border p-4 text-center">
          <Activity className="w-5 h-5 text-gray-400 mx-auto mb-1" />
          <p className="text-xs text-gray-500">Status</p>
          <div className="mt-1"><StatusBadge status={ticket.status} /></div>
        </div>
        <div className="bg-white rounded-xl border p-4 text-center">
          <AlertCircle className="w-5 h-5 text-gray-400 mx-auto mb-1" />
          <p className="text-xs text-gray-500">Priority</p>
          <div className="mt-1"><PriorityBadge priority={ticket.priority} /></div>
        </div>
        <div className="bg-white rounded-xl border p-4 text-center">
          <ThumbsUp className="w-5 h-5 text-gray-400 mx-auto mb-1" />
          <p className="text-xs text-gray-500">Satisfaction</p>
          <p className="text-lg font-bold text-gray-900">—</p>
        </div>
      </div>

      {/* Close Ticket (if not resolved) */}
      {!isResolved && (
        <Card title="Close Ticket" subtitle="Provide a resolution summary to close this ticket">
          <div className="space-y-4">
            <textarea rows={4} value={resolutionSummary} onChange={e => setResolutionSummary(e.target.value)}
              className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm"
              placeholder="Describe how this issue was resolved..." />
            <button onClick={handleResolve} disabled={!resolutionSummary || resolving}
              className="inline-flex items-center gap-2 px-4 py-2 bg-green-600 text-white rounded-lg text-sm font-medium hover:bg-green-700 disabled:opacity-50">
              {resolving ? <Loader2 className="w-4 h-4 animate-spin" /> : <CheckCircle2 className="w-4 h-4" />}
              {resolving ? 'Resolving...' : 'Mark as Resolved'}
            </button>
          </div>
        </Card>
      )}

      {/* Audit Trail */}
      <Card title="Audit Trail" subtitle="Complete history of actions on this ticket">
        {auditTrail.length === 0 ? (
          <p className="text-sm text-gray-500">No audit entries yet.</p>
        ) : (
          <div className="relative">
            <div className="absolute left-4 top-0 bottom-0 w-px bg-gray-200" />
            <div className="space-y-4">
              {auditTrail.map((entry, i) => (
                <div key={entry.id} className="relative flex items-start gap-4 pl-10">
                  <div className={`absolute left-2.5 w-3 h-3 rounded-full border-2 border-white shadow ${
                    entry.action === 'Resolved' ? 'bg-green-500' :
                    entry.action === 'Escalated' ? 'bg-orange-500' :
                    entry.action === 'AI Triage' ? 'bg-blue-500' :
                    entry.action === 'Created' ? 'bg-purple-500' :
                    'bg-gray-400'
                  }`} />
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      <span className="text-sm font-medium text-gray-900">{entry.action}</span>
                      <span className="text-xs text-gray-400">•</span>
                      <span className="text-xs text-gray-500">{entry.actor}</span>
                    </div>
                    <p className="text-sm text-gray-600 mt-0.5">{entry.details}</p>
                    {(entry.previousValue || entry.newValue) && (
                      <p className="text-xs text-gray-400 mt-0.5">
                        {entry.previousValue && <span>{entry.previousValue}</span>}
                        {entry.previousValue && entry.newValue && <span> → </span>}
                        {entry.newValue && <span className="font-medium">{entry.newValue}</span>}
                      </p>
                    )}
                    <p className="text-xs text-gray-400 mt-1">
                      {new Date(entry.timestamp).toLocaleString()}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </Card>

      {/* Ticket Info */}
      <Card title="Ticket Details">
        <div className="grid grid-cols-2 gap-4 text-sm">
          <div><span className="text-gray-500">Requester:</span> <span className="font-medium ml-2">{ticket.requesterName}</span></div>
          <div><span className="text-gray-500">Department:</span> <span className="font-medium ml-2">{ticket.requesterDepartment}</span></div>
          <div><span className="text-gray-500">Email:</span> <span className="font-medium ml-2">{ticket.requesterEmail}</span></div>
          <div><span className="text-gray-500">Assignee:</span> <span className="font-medium ml-2">{ticket.assignee || 'Unassigned'}</span></div>
          <div><span className="text-gray-500">Created:</span> <span className="font-medium ml-2">{new Date(ticket.createdAt).toLocaleString()}</span></div>
          <div><span className="text-gray-500">Last Updated:</span> <span className="font-medium ml-2">{new Date(ticket.updatedAt).toLocaleString()}</span></div>
        </div>
      </Card>
    </div>
  );
}
