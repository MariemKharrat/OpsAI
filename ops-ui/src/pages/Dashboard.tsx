import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { BarChart3, AlertTriangle, CheckCircle2, Clock, Ticket } from 'lucide-react';
import { getStats } from '../services/api';
import { StatCard, Card } from '../components/Card';
import { PriorityBadge, StatusBadge, CategoryBadge } from '../components/Badge';
import type { DashboardStats } from '../types';

export default function Dashboard() {
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    getStats().then(r => { setStats(r.data); setLoading(false); })
      .catch(() => setLoading(false));
  }, []);

  if (loading) return <LoadingSkeleton />;
  if (!stats) return <p className="text-red-500">Failed to load dashboard data.</p>;

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p className="text-gray-500 mt-1">IT Support Operations Overview</p>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          label="Total Tickets"
          value={stats.totalTickets}
          icon={<Ticket className="w-6 h-6 text-blue-600" />}
        />
        <StatCard
          label="Open Tickets"
          value={stats.openTickets}
          icon={<Clock className="w-6 h-6 text-blue-600" />}
        />
        <StatCard
          label="Critical Issues"
          value={stats.criticalTickets}
          icon={<AlertTriangle className="w-6 h-6 text-red-500" />}
        />
        <StatCard
          label="Resolved Today"
          value={stats.resolvedToday}
          icon={<CheckCircle2 className="w-6 h-6 text-green-500" />}
        />
      </div>

      {/* Charts Row */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <Card title="By Status">
          <div className="space-y-3">
            {Object.entries(stats.byStatus).map(([status, count]) => (
              <div key={status} className="flex items-center justify-between">
                <span className="text-sm text-gray-600">{status}</span>
                <div className="flex items-center gap-2">
                  <div className="w-24 bg-gray-100 rounded-full h-2">
                    <div
                      className="bg-blue-500 h-2 rounded-full"
                      style={{ width: `${(count / stats.totalTickets) * 100}%` }}
                    />
                  </div>
                  <span className="text-sm font-medium w-6 text-right">{count}</span>
                </div>
              </div>
            ))}
          </div>
        </Card>

        <Card title="By Priority">
          <div className="space-y-3">
            {Object.entries(stats.byPriority).map(([priority, count]) => {
              const colors: Record<string, string> = { Critical: 'bg-red-500', High: 'bg-orange-500', Medium: 'bg-blue-500', Low: 'bg-gray-400' };
              return (
                <div key={priority} className="flex items-center justify-between">
                  <span className="text-sm text-gray-600">{priority}</span>
                  <div className="flex items-center gap-2">
                    <div className="w-24 bg-gray-100 rounded-full h-2">
                      <div className={`${colors[priority] || 'bg-gray-400'} h-2 rounded-full`}
                        style={{ width: `${(count / stats.totalTickets) * 100}%` }} />
                    </div>
                    <span className="text-sm font-medium w-6 text-right">{count}</span>
                  </div>
                </div>
              );
            })}
          </div>
        </Card>

        <Card title="By Category">
          <div className="space-y-3">
            {Object.entries(stats.byCategory).sort((a, b) => b[1] - a[1]).slice(0, 6).map(([cat, count]) => (
              <div key={cat} className="flex items-center justify-between">
                <span className="text-sm text-gray-600">{cat}</span>
                <span className="text-sm font-medium">{count}</span>
              </div>
            ))}
          </div>
        </Card>
      </div>

      {/* Recent Tickets */}
      <Card title="Recent Tickets" subtitle="Latest tickets requiring attention"
        action={
          <button onClick={() => navigate('/tickets/new')}
            className="text-sm text-blue-600 hover:text-blue-700 font-medium">
            + New Ticket
          </button>
        }>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="text-left text-gray-500 border-b">
                <th className="pb-3 font-medium">Ticket</th>
                <th className="pb-3 font-medium">Subject</th>
                <th className="pb-3 font-medium">Status</th>
                <th className="pb-3 font-medium">Priority</th>
                <th className="pb-3 font-medium">Category</th>
                <th className="pb-3 font-medium">Requester</th>
                <th className="pb-3 font-medium">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {stats.recentTickets.map(ticket => (
                <tr key={ticket.id} className="hover:bg-gray-50">
                  <td className="py-3 font-mono text-xs text-gray-500">{ticket.ticketNumber}</td>
                  <td className="py-3 max-w-xs truncate">{ticket.subject}</td>
                  <td className="py-3"><StatusBadge status={ticket.status} /></td>
                  <td className="py-3"><PriorityBadge priority={ticket.priority} /></td>
                  <td className="py-3"><CategoryBadge category={ticket.category} /></td>
                  <td className="py-3 text-gray-600">{ticket.requesterName}</td>
                  <td className="py-3">
                    <button onClick={() => navigate(`/tickets/${ticket.id}/workspace`)}
                      className="text-blue-600 hover:text-blue-700 text-xs font-medium">
                      Open
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  );
}

function LoadingSkeleton() {
  return (
    <div className="space-y-8 animate-pulse">
      <div className="h-8 w-48 bg-gray-200 rounded" />
      <div className="grid grid-cols-4 gap-4">
        {[1,2,3,4].map(i => <div key={i} className="h-24 bg-gray-200 rounded-xl" />)}
      </div>
      <div className="h-64 bg-gray-200 rounded-xl" />
    </div>
  );
}
